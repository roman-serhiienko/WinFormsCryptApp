using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Xml.Linq;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace FileCryptWinApp
{
        //class FCrypt
        //partial class Form1        
        public partial class Form1 : Form                 //change this to make working SetStatus method
        {
                private const int timeout = 1000;         //Timeout to read notifications, milliseconds
                private const int timeout2 = 5000;        //Timeout to read notifications, milliseconds
                private RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();                                        //RSA
                
                private int bitlength = 4096;             //4096 bits is bitlength for RSA-keys, by default. 4096 [bits] / 8 [bits/bytes] = 512 [bytes] - in each block of cyphertext.
                //private int bitlength = 512;            //faster for testing
        
                private string pub_file = "public_key.xml";                                           //default filename for public key
                private string priv_file = "private_key.xml";                                         //default filename for private key
                private string key_iv_file = "key_iv.xml";                                            //default filename for not encrypted xml keyfile with Key and Initialization vector
                private string encrypted_key_iv_file = "encrypted_key_iv.xml";                        //default filename for RSA encrypted xml keyfile with AES values, SHA256-hash + HMAC signature.
                private string keyfiles_pass = "Do_not_change_it_to_decrypt_KEY_IV_from_keyfile";     //default password to load KEY and IV from encrypted key-file (not from password), and decrypt it with RSA private key.
                private string default_password = "YOUR_password";                                    //default password to display.
                private        byte [] key = null;                                                    //private byte array for key
                private byte [] iv = null;                                                            //private byte array for iv
                private string password = "";                                                         //current password string

                
                // declaration & instantiation of initialization vector
//                static byte[] initVector = new byte[] { 0x01, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 
//                                                                         0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

                static byte[] initVector = new byte[] { };                                            //define this as empty bytearray, and generate this from password in future.

                static SymmetricAlgorithm sa;                                                         //Just define this.

                /// <summary>
                /// Create encryption information in the form of xml string
                /// </summary>
                /// <param name="signatureKey">Signature Key</param>
                /// <param name="encryptionKey">AES Encryption key</param>
                /// <param name="encryptionIV">AES Encryption key IV</param>
                /// <returns>xml string containing key informations</returns>
                private string[] CreateEncryptionInfoXml(byte[] encryptionKey, byte[] encryptionIV)   //create XML keyfile with AES parameterd (Key and IV) + hash + return HMAC signature
                {
                        string template =                                                             //Create XML-template
                                        "<EncryptionInfo>"
                                +                "<AESKeyValue>"
                                +                        "<Key/>"                                     //AES Key, base64 encoded
                                +                        "<IV/>"                                      //Initialization vector, base64 encoded
                                +                "</AESKeyValue>"
                                +        "</EncryptionInfo>"
                        ;

                        XDocument doc = XDocument.Parse(template);                                                                              //parse this and add values
                        doc.Descendants("AESKeyValue").Single().Descendants("Key").Single().Value = Convert.ToBase64String(encryptionKey);      //Key
                        doc.Descendants("AESKeyValue").Single().Descendants("IV").Single().Value = Convert.ToBase64String(encryptionIV);        //IV

                        string xml = doc.ToString();                                                  //XML-keyfile as xml-string.
                        
                        //calculate hash
                        string sha256_XML = SHA256_hash(xml);                                         //sha hash from this as string (separate method here)
                        
                        //HMAC signature
                        string HMAC_key = SHA256_hash(xml+sha256_XML);                                //HMAC key to verify signature. Verification of signature, is possible, after decrypt, of course.
                        byte[] sig = null;                                                            //define bytearray for signature.
                        using (
                                        HMACSHA256 sha =
                                        new HMACSHA256(                                                       //compute HMAC signature
                                                                        new UTF8Encoding(true).GetBytes(
                                                                                HMAC_key                      //string of key as bytes
                                                                        )
                                        )
                        )
                        {
                                sig = sha.ComputeHash(new UTF8Encoding(true).GetBytes(xml));                  //Calculate HMAC-signature, byte []
                        }
                        
                        //doc.Save(key_iv_file);                                                              //Save key and iv as xml, without RSA encryption. Just for test.

                        //StreamWriter xml_stream = System.IO.File.CreateText(key_iv_file);                   //If file is locked somewhere
                        //doc.Save(xml_stream);
                        //xml_stream.Close();                                                                 //possible to close this stream.
                        
                        return new string[] {                                                                 //return xml string with base64 encoded AES Key and IV + HMAC signature.
                                xml,                                                                          //xml-string
                                Convert.ToBase64String(new UTF8Encoding(true).GetBytes(HMAC_key))             //HMAC signature. HMAC key = SHA256(xml+sha256)
                        };
                }

                private string SHA256_hash(string data){                                                      //sha256 hash from string - as string
                        //calculate hash
                        byte[] sha256_bytes = SHA256Managed.Create().ComputeHash(new UTF8Encoding(true).GetBytes(data));                //sha256 bytes
                        StringBuilder result = new StringBuilder();                                                                     //to string
                        for (int i = 0; i < sha256_bytes.Length; i++)        result.Append(sha256_bytes[i].ToString("X2"));             //all this bytes
                        return result.ToString();                                                                                       //and return this string
                }
                
                //compare the hash inside encrypted XML-Keyfile with hash of encrypted data inside this encrypted XML-keyfile.
                private bool verify_sha256hash(XDocument Enc){                                                         //bool - true/false. Sha256 is equals or not.
                        
                        string sha256_hash_in_file =
                                Enc.
                                Descendants("EncryptedEncryptionInfo").Single().
                                Descendants("SHA256").Single().                                                        //SHA256 hash from xml-file with encrypted values
                                Value
                        ;

                        string need_to_hashing =
                                Enc.
                                Descendants("EncryptedEncryptionInfo").Single().
                                Descendants("EncryptedAESKeyValue").SingleOrDefault()                                  //the part of xml with encrypted key and IV
                                .ToString()
                        ;
                        
                        need_to_hashing =
                                String.Concat(
                                                need_to_hashing,
                                                Enc
                                                        .Descendants("EncryptedEncryptionInfo")
                                                        .Descendants("HMACSHA256").SingleOrDefault()
                                                        .ToString()                                                    //concat previous string with xml of HMAC signature + keyInfo.
                                );

                        string sha256_encrypted = SHA256_hash(need_to_hashing);                                        //compute SHA256 hash from total xml-string
                        return (sha256_hash_in_file==sha256_encrypted);                                                //return result of comparation. true/false
                }

                private void CryptAlgInit()                                                                            // instantiation the algorithm of encryption
                {
                        sa = Aes.Create();                                                                             //defining the algorithm of encryption - AES 
                        sa.Mode = Form1.cipherMode;                                                                    //mode of blocks chaining while the set of block encryption
                        sa.KeySize = Form1.keySize ;                                                                   //size of cipher key: 128, 192 or 256
                        sa.BlockSize = Form1.blockSize ;                                                               //size blocksize: constantly 128 bit = 16 bytes. This is the length of iv.
                        
                }

                private string Reverse( string s )                                                                     //Separate method to get reverse string. Just to get dynamic IV from password.
                {
                        char[] charArray = s.ToCharArray();
                        Array.Reverse( charArray );
                        return new string( charArray );
                }
                
                private void SetStatus(string status)                                                                  //set status in lblStatus, and split this, if long.
                {
                        string[] notifications = new string[20];
                        //Console.WriteLine(status.Length);                                                            //Console.WriteLine working, if program will be compiled without key csc.exe -target:winexe
                        if(status.Length>=180)                                                                         //if string so long
                        {
                                System.Threading.Thread.Sleep(timeout2);
                                //Console.WriteLine("Old status: "+status);                                            //show old
                                notifications = status.Split(new string[] { "! " }, StringSplitOptions.None);          //split this
                                string new_status =
                                        notifications[notifications.Length-3]+"! "+
                                        notifications[notifications.Length-2]+"! "+                                    //add to string the last notifications
                                        notifications[notifications.Length-1]
                                ;
                                //Console.WriteLine("New status: "+new_status+" notifications.Length"+notifications.Length);        //show new
                                lblStatus.Text = new_status;                                                           //and append string to status bar
                        }else{
                                lblStatus.Text = status;                                                               //else append full string
                        }
                        lblStatus.Invalidate();
                        Application.DoEvents();
                }
                
                private void Generate_RSA_keys()                                                             //Generate RSA keys, if this can not be loaded.
                {
                        SetStatus(this.lblStatus.Text+"Wait for generate RSA keys!"+
                                "(Key size bitlength = "+bitlength+" bits)! ");                              //show status
                        System.Threading.Thread.Sleep(timeout);                                              //sleep to make this readable.
                
                        try{
                                rsa = new RSACryptoServiceProvider(bitlength);                               //bitlength was defined earlier
                                RSAParameters pa = rsa.ExportParameters(true);                               //true - means private key
                                SetStatus(this.lblStatus.Text+"New key pair generated! BitLength: "+
                                        bitlength+" bits ("+bitlength/8+" bytes)! ");                        //done.
                        }catch (Exception ex){
                                SetStatus(this.lblStatus.Text+"Error:" + ex.Message + "! ");                 //else - error
                        }
                        System.Threading.Thread.Sleep(timeout);                                              //sleep, after last status displayed.
                        return;                                                                              //and return.
                }
        
                private void btnSavePrivate_Click(string path = "")                                          //save RSA private key
                {
                        try
                        {
                                string filename = "";                                                        //define empty string
                                if(path==""){
                                        SetStatus("Input filename to save PRIVATE key, or cancel to auto-save! ");
                                        System.Threading.Thread.Sleep(timeout);
                                        
                                        SaveFileDialog s = new SaveFileDialog();                                     //input filename
                                        s.Filter = "RSA Key Files(*.xml)|*.xml";
                                        if (s.ShowDialog() == DialogResult.OK){
                                                filename = s.FileName;                                               //and save there
                                                SetStatus(this.lblStatus.Text+"Saving private key, there! ");
                                        }else{
                                                filename = priv_file;                                                //or use default filename
                                                SetStatus(this.lblStatus.Text+
                                                "Saving private key, with default filename! ");
                                        }
                                        System.Threading.Thread.Sleep(timeout);                                      //sleep, after status.
                                }else{
                                                filename = path;                                                     //or use default filename
                                                SetStatus(this.lblStatus.Text+
                                                "Saving private key, as "+path+"! ");                                
                                }
                                if (System.IO.File.Exists(filename))
                                        System.IO.File.Delete(filename);                                             //delete filename if exists
                                System.IO.StreamWriter writer = System.IO.File.CreateText(filename);                 //and open this to write

                                string xml_priv =         rsa.ToXmlString(true)                                      //xml string of private key
                                                                        .Replace("<", "\n\t<")                       //replaced to be readable.
                                                                        .Replace(">",">\n\t\t")
                                                                        .Replace("\t\t\n","")
                                                                        .Replace("\t</RSAKeyValue>\n\t\t","</RSAKeyValue>")
                                                                        .Replace("\n\t<R","<R")
                                ;
                                writer.Write(xml_priv);                                                              //write this to file.
                        
                                writer.Close();                                                                      //close file
                                SetStatus(this.lblStatus.Text+
                                        "Done! Private key saved to: " + filename + "! ");                           //done.
                        }
                        catch (Exception ex)
                        {
                                SetStatus(this.lblStatus.Text+"Error:" + ex.Message + "! ");                         //else show error
                        }
                        System.Threading.Thread.Sleep(timeout);                                                      //sleep
                }

                private void btnLoadPrivate_Click(string path="")                                                    //Load RSA privkey. Default path is empty string.
                {
                        try
                        {
                                string filename = "";                                                                //define empty string
                                if(path==""){
                                        SetStatus(this.lblStatus.Text+"Select file with private key! ");             //show status
                                        System.Threading.Thread.Sleep(timeout);                                      //sleep then

                                        OpenFileDialog o = new OpenFileDialog();                                     //select file
                                        o.Filter = "RSA Key Files(*.xml)|*.xml";
                                        if (o.ShowDialog() == DialogResult.OK)
                                        {
                                                filename = o.FileName;                                               //use selected
                                                SetStatus(this.lblStatus.Text+"Load specified the public key! ");
                                        }
                                        else{
                                                filename = priv_file;                                                //or default
                                                SetStatus(this.lblStatus.Text+"load the public from default pathway! ");
                                        }
                                }else{
                                        filename = path;                                                             //or spefied path
                                        SetStatus(this.lblStatus.Text+"load the public from "+path);
                                }
                                System.Threading.Thread.Sleep(timeout);                                              //timeout
                                System.IO.StreamReader reader = System.IO.File.OpenText(filename);                   //open this
                                string s = reader.ReadToEnd();
                                rsa.FromXmlString(s);                                                                //import privkey there.
                                reader.Close();
                                SetStatus(this.lblStatus.Text+"Private key loaded! ");
                        }
                        catch (Exception ex)
                        {
                                SetStatus(this.lblStatus.Text+"Error:" + ex.Message+"! ");                           //or error
//                                thread.Abort();
//                                GoToReadyMode();
                        }
                        System.Threading.Thread.Sleep(timeout);
                }

                private void btnSavePublic_Click(string path = "")                                                   //save RSA public key. Default path = ""
                {
                        try
                        {
                                string filename = "";                                                                //define empty string
                                if(path==""){
                                        SetStatus("Input filename to save PUBLIC key, or cancel to auto-save! ");    //status
                                        System.Threading.Thread.Sleep(timeout);
                                        
                                        SaveFileDialog s = new SaveFileDialog();                                     //input filename
                                        s.Filter = "RSA Key Files(*.xml)|*.xml";                                     //with fiter of types
                                        if (s.ShowDialog() == DialogResult.OK){
                                                filename = s.FileName;                                               //specified path
                                                SetStatus(this.lblStatus.Text+"Save the public key there! ");
                                        }
                                        else{
                                                filename = pub_file;                                                 //or default pathway
                                                SetStatus(this.lblStatus.Text+
                                                "Save the public key by default filename! ");
                                        }
                                }else{
                                                filename = path;                                                     //or default pathway
                                                SetStatus(this.lblStatus.Text+
                                                "Save the public key as "+path+"! ");                                        
                                }

                                System.Threading.Thread.Sleep(timeout);                                              //sleep

                                if (System.IO.File.Exists(filename)) System.IO.File.Delete(filename);                //delete file if exists
                                System.IO.StreamWriter writer = System.IO.File.CreateText(filename);                 //open to write

                                string xml_pub =         rsa.ToXmlString(false)                                      //false means not private key, public.
                                                                        .Replace("<", "\n\t<")
                                                                        .Replace(">",">\n\t\t")
                                                                        .Replace("\t\t\n","")
                                                                        .Replace("\t</RSAKeyValue>\n\t\t","</RSAKeyValue>")
                                                                        .Replace("\n\t<R","<R")
                                ;
                                writer.Write(xml_pub);                                                               //write this.
                        
                                writer.Close();
                                SetStatus(this.lblStatus.Text+"Done! Public key saved to: " + filename+"! ");        //done
                        }
                        catch (Exception ex)
                        {
                                SetStatus(this.lblStatus.Text+"Error:" + ex.Message + "! ");                         //or error
                                //thread.Abort();
                                //GoToReadyMode();
                        }
                        System.Threading.Thread.Sleep(timeout);                                                      //sleep
                }

                private void load_or_generate_and_save_RSA_keys(){                                                   //if RSA keys exists - load this, or generate/import and save.
                        if(File.Exists(priv_file)){                                                                  //if priv exists
                                SetStatus(this.lblStatus.Text+"Private key found - try to load it! ");
                                System.Threading.Thread.Sleep(timeout);
                                btnLoadPrivate_Click(priv_file);                                                     //load this
                                btnSavePublic_Click(pub_file);                                                       //and import pub
                        }
                        else if(File.Exists(pub_file)){                                                              //if pub exists
                                        //wanted to load public only for encrypt, because decrypt data is possible by priv.
                                System.IO.StreamReader reader = System.IO.File.OpenText(pub_file);
                                string s = reader.ReadToEnd();
                                rsa.FromXmlString(s);                                                                //import this
                                reader.Close();
                                SetStatus(this.lblStatus.Text+"Public key loaded! "
                                        +        "You don't have the private key! "
                                        +        "You can encrypt only, or delete this! "                            //only for encrypt
                                );
                                System.Threading.Thread.Sleep(timeout);
                        }
                        else{                                                                                        //else
                                SetStatus(this.lblStatus.Text+priv_file+" and "+pub_file+" - not found! ");
                                System.Threading.Thread.Sleep(timeout);
                                SetStatus(this.lblStatus.Text+"No any keys to import! ");
                                System.Threading.Thread.Sleep(timeout);
                                SetStatus(this.lblStatus.Text+"Generate keys! ");
                                System.Threading.Thread.Sleep(timeout);
                                Generate_RSA_keys();                                                                 //generate RSA keypair
                                btnSavePrivate_Click(priv_file);                                                     //ans save both
                                btnSavePublic_Click(pub_file);                                                       //RSA keys - priv and pub.
                        }
                        return;
                }

                private bool load_or_generate_and_save_AES_keys(string pass){                                        //load or generate AES key, and save this, after RSA encryption
                        try{
                                string [] key_iv_and_sig = null;                                                     //define bytearray                                        
                                string hmac_sig_calculated = null;                                                   //and string
                                
                                this.password = pass;                                                                //set password
                                
                                if(                File.Exists(encrypted_key_iv_file)                                //if encrypted key_IV exists
                                        &&        File.Exists(priv_file)                                             //and if priv exists
                                        &&        (this.password == this.keyfiles_pass)                              //and if need to load keyfile
                                ){        
                                                        //decrypt key and iv, with priv, from encrypted xml file.
                                
                                        SetStatus(this.lblStatus.Text+"Try to decrypt encrypted keyfile! ");
                                        System.Threading.Thread.Sleep(timeout);
                                        XDocument doc = XDocument.Load(encrypted_key_iv_file);                       //encrypted key as xml document
                                        
                                        //Calculate sha256 hash of encrypted data and compare with specified hash
                                        SetStatus(
                                                this.lblStatus.Text + ((verify_sha256hash(doc)==true)
                                                        ? "SHA256 hash verified! "                                           //If comparison == true
                                                        : "Sha256 hash not verified... Try to decrypt key and vector... "    //If comparison == false
                                                )
                                        );
                                        System.Threading.Thread.Sleep(timeout);
                                        
                                        string encrypted_key = doc.
                                                                        Descendants("EncryptedEncryptionInfo").Single().
                                                                        Descendants("EncryptedAESKeyValue").Single().
                                                                        Descendants("EncryptedKey").Single().                //get encrypted AES Key
                                                                        Value
                                        ;
                                        //Console.WriteLine("encrypted_key: "+encrypted_key);
                                        
                                        string encrypted_iv =         doc.
                                                                        Descendants("EncryptedEncryptionInfo").Single().
                                                                        Descendants("EncryptedAESKeyValue").Single().
                                                                        Descendants("EncryptedIV").Single().                 //get encrypted IV
                                                                        Value
                                        ;
                                        //Console.WriteLine("encrypted_iv: "+encrypted_iv);
                                        
                                        string HMAC_sig_from_file =         doc.
                                                                        Descendants("EncryptedEncryptionInfo").Single().
                                                                        Descendants("HMACSHA256_signature").Single().        //get HMAC signature.
                                                                        Value
                                        ;
                                        //Console.WriteLine("HMAC_sig_from_file: "+HMAC_sig_from_file);
                                        
                                        load_or_generate_and_save_RSA_keys();                                                //load RSA private key if exists to decrypt
                                        key = rsa.Decrypt(System.Convert.FromBase64String(encrypted_key), false);            //decrypt AES-key, with RSA-priv
                                        //Console.WriteLine("decrypted key: "+Convert.ToBase64String(key));

                                        iv = rsa.Decrypt(System.Convert.FromBase64String(encrypted_iv), false);              //decrypt AES-IV, with RSA-priv
                                        //Console.WriteLine("decrypted iv: "+Convert.ToBase64String(iv));

                                        key_iv_and_sig = CreateEncryptionInfoXml(key, iv);                                   //return key and IV as xml in string + HMAC signature
                                        hmac_sig_calculated = key_iv_and_sig[1];                                             //get HMAC signature, after calculating
                                        //Console.WriteLine("(HMAC_sig_from_file==hmac_sig_calculated): "+(HMAC_sig_from_file==hmac_sig_calculated));

                                        if(HMAC_sig_from_file==hmac_sig_calculated){                                         //If comparison == true
                                                SetStatus(this.lblStatus+"HMAC signature verified! ");                       //show this
                                                System.Threading.Thread.Sleep(timeout);                                      //sleep
                                                SetStatus(this.lblStatus+"AES key and iv successfully loaded! ");            //show this
                                                System.Threading.Thread.Sleep(timeout);                                      //sleep
                                                return true;                                                                 //and continue... Key loaded.
                                        }else{
                                                SetStatus(this.lblStatus+"HMAC signature not verified... Invalid key... Stop! ");    //If comparison == false - error
                                                System.Threading.Thread.Sleep(timeout);                                      //sleep
                                                //wanted to return false, but this method have CryptoStream type
                                                return false;                                                                //return false. Cann't decrypt. Key not loaded.
                                        }
                                }else if(
                                                File.Exists(key_iv_file)                                                             //if not encrypted xml-file with base64-encoded AES Key IV founded
                                        &&        (this.password == this.keyfiles_pass)                                              //and if need to use keyfile
                                ){                                                                                           //import key and iv from not encrypted xml-file
                                        XDocument doc = XDocument.Load(key_iv_file);
                                        key = System.Convert.FromBase64String(
                                                        doc.        Descendants("EncryptionInfo").Single().
                                                                        Descendants("AESKeyValue").Single().
                                                                        Descendants("Key").Single().                         //AES Key
                                                                        Value
                                        )
                                        ;
                                        iv = System.Convert.FromBase64String(
                                                        doc.        Descendants("EncryptionInfo").Single().
                                                                        Descendants("AESKeyValue").Single().
                                                                        Descendants("IV").Single().                          //AES IV
                                                                        Value
                                        )
                                        ;
                                        SetStatus(this.lblStatus+"AES KEY loaded from not encrypted keyfile! ");             //If comparison == false - error
                                        System.Threading.Thread.Sleep(timeout);                                              //sleep
                                }else if (
                                                !File.Exists(this.key_iv_file)                                               //keyfiles
                                        ||                                                                                   //not
                                                (                                                                            //exists
                                                        !File.Exists(this.encrypted_key_iv_file) || !File.Exists(this.priv_file)     //or cann't be decrypted
                                                )
                                        ||        this.password!=this.keyfiles_pass                                                  //or if no need to use keyfiles
                                )
                                {                                                                                                    //just generate key and iv from password.
                                        SetStatus(this.lblStatus.Text+"Keyfile not used! ");
                                        System.Threading.Thread.Sleep(timeout);
                                        SetStatus(this.lblStatus.Text+"Generate Key+IV from password! ");
                                        System.Threading.Thread.Sleep(timeout);

                                        CryptAlgInit();

                                        this.key = (new PasswordDeriveBytes(this.password, null)).GetBytes(sa.KeySize/8);            //get keySize/8 bytes
                                        //initialization vector:
                                        byte [] bytes = SHA256.Create().ComputeHash(new MemoryStream((new PasswordDeriveBytes(Reverse(this.password), null)).GetBytes(sa.KeySize/8)));
                                        this.iv = new byte[sa.BlockSize/8];                                //iv.length = blocksize = 128 bit = 16 bytes = const, even for AES-256
                                        Array.Copy(bytes, this.iv, this.iv.Length);
                                        bytes = new byte[0];                                                                         //remove data there

                                        SetStatus(this.lblStatus.Text+"Done! ");
                                        System.Threading.Thread.Sleep(timeout);
                                }else{
                                        SetStatus(this.lblStatus+"load_or_generate_and_save_AES_keys: Something else, return false... ");        //If comparison == false - error
                                        System.Threading.Thread.Sleep(timeout);                                                                  //sleep
                                        return false;
                                }
                                
                                //after importing key... Try to save it
                                //save Key and IV as xml-file.
                                key_iv_and_sig = CreateEncryptionInfoXml(key, iv);                                   //return key and IV + HMAC sig - as xml in string
                                //string key_iv = key_iv_and_sig[0];                                                 //copy xml string with keys
                                string HMAC_sig = key_iv_and_sig[1];
                                key_iv_and_sig = new string[0];                                                      //delete this if xml will not be writed without encryption
                                
                                //if (!File.Exists(key_iv_file))//if keyfile not exists
                                //{Just uncomment doc.Save(key_iv_file); in CreateEncryptionInfoXml}
                                //else
                                if (!File.Exists(encrypted_key_iv_file))                                             //if encrypted keyfile not exists
                                {
                                                SetStatus(this.lblStatus.Text+"Encrypted keyfile not found! ");
                                                System.Threading.Thread.Sleep(timeout);
                                                SetStatus(this.lblStatus.Text+"Try to save it! ");
                                                System.Threading.Thread.Sleep(timeout);
                                                //doc.Save(key_iv_file); commented in CreateEncryptionInfoXml method, so KeyIV will be saved after RSA-encryption.
                                                load_or_generate_and_save_RSA_keys();                                 //load or generate and save RSA keys.
                                                
                                                //byte[] key_iv_xml = new UTF8Encoding(true).GetBytes(key_iv);        //utf-8 encoded XML -> to bytes. This no need, if xml will not be writed without encryption.

                                                string template =                                                     //Create template for encrypted xml keyfile.
                                                                "<EncryptedEncryptionInfo>"
                                                        +                "<KeysEncryptionAlgorithm>"
                                                        +                "</KeysEncryptionAlgorithm>"
                                                        +                "<EncryptedAESKeyValue>"
                                                        +                        "<EncryptedKey/>"
                                                        +                        "<EncryptedIV/>"
                                                        +                "</EncryptedAESKeyValue>"
                                                        +                "<HMACSHA256>"
                                                        +                        "<HMACSHA256_key/>"
                                                        +                        "<HMACSHA256_signature/>"
                                                        +                "</HMACSHA256>"
                                                        +                "<SHA256/>"
                                                        +        "</EncryptedEncryptionInfo>"
                                                ;

                                                XDocument doc2 = XDocument.Parse(template);                                   //parse this and fill values
                                                
                                                doc2.Descendants("EncryptedEncryptionInfo").Single()
                                                .Descendants("KeysEncryptionAlgorithm").Single()
                                                .Value = "RSA "+bitlength+" bits.";                                           //RSA bitlength -> to XML
                                                
                                                //Console.WriteLine("RSA "+bitlength+" bits. - writted.");
                                                string encrypted_key = Convert.ToBase64String(rsa.Encrypt(key, false));       //RSA encrypted AES Key
                                                //Console.WriteLine("encrypted_key (base64 string): "+encrypted_key);
                                                
                                                doc2.Descendants("EncryptedEncryptionInfo").Single()
                                                .Descendants("EncryptedAESKeyValue").Single()
                                                .Descendants("EncryptedKey").Single()
                                                .Value = encrypted_key;                                                       //-> to XML
                                                
                                                //Console.WriteLine("encrypted Key - writted.");
                                                
                                                string encrypted_IV = Convert.ToBase64String(rsa.Encrypt(iv, false));         //RSA encrypted AES IV
                                                //Console.WriteLine("encrypted_IV (base64 string): "+encrypted_IV);
                                                
                                                doc2.Descendants("EncryptedEncryptionInfo").Single()
                                                .Descendants("EncryptedAESKeyValue").Single()
                                                .Descendants("EncryptedIV").Single()
                                                .Value = encrypted_IV;                                                        //-> to XML
                                                
                                                //Console.WriteLine("encryptedIV - writted.");
                                                
                                                doc2.Descendants("EncryptedEncryptionInfo").Single()                          //info about HMAC key
                                                .Descendants("HMACSHA256").Single()
                                                .Descendants("HMACSHA256_key")
                                                .Single().Value = "SHA256( decrypted_xml + SHA256( decrypted_xml ) );";       // -> to XML
                                                
                                                //Console.WriteLine("result.ToString() - writted.");
                                                doc2.Descendants("EncryptedEncryptionInfo").Single()                          //HMAC signature
                                                .Descendants("HMACSHA256").Single()
                                                .Descendants("HMACSHA256_signature").Single()
                                                .Value = HMAC_sig;                                                            // -> to XML
                                                //Console.WriteLine("result.ToString() - writted.");

                                                string need_to_hashing = doc2.
                                                Descendants("EncryptedEncryptionInfo").Single()
                                                .Descendants("EncryptedAESKeyValue").SingleOrDefault()
                                                .ToString();                                                                  //encrypted key and IV
                                                
                                                need_to_hashing = String.Concat(need_to_hashing, doc2
                                                .Descendants("EncryptedEncryptionInfo").Single()
                                                .Descendants("HMACSHA256").SingleOrDefault().ToString());                     //+ HMAC info - in one string for hashing
                                                
                                                string sha256_encrypted_key_IV = SHA256_hash(need_to_hashing);                //compute sha256-hash
                                                //Console.WriteLine("..........NEED To sha256: "+need_to_hashing+"\nHash: "+sha256_encrypted_key_IV);
                                                doc2.Descendants("EncryptedEncryptionInfo").Single()
                                                .Descendants("SHA256").Single().Value = sha256_encrypted_key_IV;              //-> to XML
                                                //Console.WriteLine("sha256_encrypted_key_IV - writted.");

                                                doc2.Save(encrypted_key_iv_file);                                             //save encrypted keyfile.

                                                //Console.WriteLine("baseString key encrypted: "+encrypted_key);
                                                //Console.WriteLine("baseString iv encrypted: "+encrypted_IV);

                                                //Console.WriteLine("Decrypted key:" + Convert.ToBase64String(rsa.Decrypt(Convert.FromBase64String(encrypted_key), false)));        //show key.
                                                //Console.WriteLine("Decrypted key:" + Convert.ToBase64String(rsa.Decrypt(Convert.FromBase64String(encrypted_IV), false)));         //show key.
                                                
                                                SetStatus(this.lblStatus.Text+"Encrypted key_iv - saved as "+encrypted_key_iv_file+"! ");                //Done!
                                                System.Threading.Thread.Sleep(timeout);
                                }

                                return true;        //Done.
                                
                        }
                        catch(Exception ex){
                                SetStatus("Error: "+ex);                                                //else error
                                System.Threading.Thread.Sleep(timeout);
                                return false;
                        }
                }
                
                
                
                private void Encrypt(string Filename, string password, string DestFname)                //encrypt file, using AES algorithm.
                {
                        byte[] data;          // data to be encrypted
                        byte[] encdata;   // data after encryption (encrypted data)

                        load_or_generate_and_save_AES_keys(password);                                   //Generate or load existing keyfile and RSA keys.

                        CryptAlgInit();                                                                 //init this to have access to sa.

                        ICryptoTransform ct = sa.CreateEncryptor(key, iv);                              //create encryptor

                        // reading data from file to "data" variable through BinaryReader
                        using (FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read, 1000))        //file stream
                        {
                                using (BinaryReader br = new BinaryReader(fs))
                                {
                                        data = br.ReadBytes((int)(fs.Length));
                                }
                        }

                        // data encryption in CryptoStream based on cryptotransform and "wrapped" in MemoryStream
                        using (MemoryStream ms = new MemoryStream()) 
                        {
                                using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))                            //cryptostream
                                {
                                //  Console.WriteLine(BitConverter.ToString(data));
                                        cs.Write(data, 0, data.Length);
                                        cs.FlushFinalBlock();
                                }
                                encdata = ms.ToArray();
                        }

         
                        // writing the encrypted data to file through BinaryWriter
                        using (FileStream fs_dest = new FileStream(DestFname, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1000))
                        {
                                using (BinaryWriter bw = new BinaryWriter(fs_dest))
                                {
                                        bw.Write(encdata);                                                                            //write encrypted data.
                                } 
                        }
                        
                        SetStatus(this.lblStatus.Text+"Encryption sucessful! ");
                        System.Threading.Thread.Sleep(timeout);
                        this.textBoxPWD.Text = ((File.Exists(this.encrypted_key_iv_file) && File.Exists(this.priv_file)) ? this.keyfiles_pass : this.default_password);
                        return;
                }

                private void Decrypt(string CryptFileName, string password, string DecryptFileName)                                   //Decrypt file, using AES algorithm.
                {
                        if(load_or_generate_and_save_AES_keys(password)==true){                                                       //run if AES key loaded, or generated,
                                //and Key + IV already exists in the fields of this class.
                                byte[] dataToDecrypt;          // data to be decrypted
                                byte[] decryptedData;   // data after decryption

                                // reading data from file to "dataToDecrypt" variable through BinaryReader
                                using (FileStream fs = new FileStream(CryptFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1000))        //source file
                                {
                                        using (BinaryReader br = new BinaryReader(fs))
                                        {
                                                dataToDecrypt = br.ReadBytes((int)(fs.Length));
                                        }
                                }

                                using (BinaryReader br2 = new BinaryReader(InternalDecrypt(dataToDecrypt, password)))                              //Using InternalDecrypt
                                { 
                                        int lengs = (int)(dataToDecrypt.Length);  // br.BaseStream.Length   was there...
                                        decryptedData = br2.ReadBytes(lengs);
                                }

                                using (FileStream fs_dest = new FileStream(DecryptFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1000))
                                {
                                        using (BinaryWriter bw = new BinaryWriter(fs_dest))
                                        {
                                                bw.Write(decryptedData);                                                                           //destination file
                                        } 
                                }
                                SetStatus(this.lblStatus.Text+"Decryption sucessful! ");
                                System.Threading.Thread.Sleep(timeout);
                        }
                        else{
                                //Console.WriteLine("Decrypt else; false");
                                SetStatus(this.lblStatus.Text+"Decrypt else !");
                                System.Threading.Thread.Sleep(timeout);
                        }
                }
          

                CryptoStream InternalDecrypt(byte[] data, string password)           //data + password
                {
                        CryptAlgInit();                                              //Init this

                        ICryptoTransform ct = sa.CreateDecryptor(key, iv);           //using key and iv from fields of this class.

                        MemoryStream ms = new MemoryStream(data);                    //write encrypted data to memorystream
                        return new CryptoStream(ms, ct, CryptoStreamMode.Read);      //decrypt this to cryptostream and return to decrypt method.
                }
                //end methods in partial class Form1.
        }
        //end partial class Form1.
}
