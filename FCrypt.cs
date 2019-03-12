using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace FileCryptWinApp
{
    class FCrypt
    {
        // declaration & instantiation of initialization vector
//        static byte[] initVector = new byte[] { 0x01, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 
//                                     0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

        static byte[] initVector = new byte[] { };
									 
        static SymmetricAlgorithm sa;

        public static void CryptAlgInit() // instantiation the algorithm of encryption
        {
            sa = Aes.Create(); // defining the algorithm of encryption - AES 
            sa.Mode = Form1.cipherMode;  // mode of blocks chaining while the set of block encryption
            sa.KeySize = Form1.keySize ;     //size of cipher key: 128, 192 or 256
        }

		public static string Reverse( string s )
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse( charArray );
			return new string( charArray );
		}
		
        public static void Encrypt(string Filename, string password, string DestFname)
        {
            byte[] data;      // data to be encrypted
            byte[] encdata;   // data after encryption (encrypted data)

            CryptAlgInit();

            // instantiation of cryptotransform according to algorithm of encryption parameters and IV
			//Key
			byte[] key = (new PasswordDeriveBytes(password, null)).GetBytes(sa.KeySize/8);
			//initialization vector:
			byte[] bytes = SHA256.Create().ComputeHash(new MemoryStream((new PasswordDeriveBytes(Reverse(password), null)).GetBytes(sa.KeySize/8)));
			byte[] iv = new byte[sa.KeySize/8];
			Array.Copy(bytes, iv, iv.Length);
			
//			Console.WriteLine("key.Length"+key.Length+", iv.Length"+iv.Length);	//this available, if compilation without -target:winexe

//            ICryptoTransform ct = sa.CreateEncryptor(
//                (new PasswordDeriveBytes(password, null)).GetBytes(16), initVector);

            ICryptoTransform ct = sa.CreateEncryptor(key, iv);

            // reading data from file to "data" variable through BinaryReader
            using (FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read, 1000))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    data = br.ReadBytes((int)(fs.Length));
                }
            }

            // data encryption in CryptoStream based on cryptotransform and "wrapped" in MemoryStream
            using (MemoryStream ms = new MemoryStream()) 
            {
                using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
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
                    bw.Write(encdata);
                } 
            }
         }

        static public void Decrypt(string CryptFileName, string password, string DecryptFileName)
        {
            byte[] dataToDecrypt;  // data to be decrypted
            byte[] decryptedData;   // data after decryption

            // reading data from file to "dataToDecrypt" variable through BinaryReader
            using (FileStream fs = new FileStream(CryptFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 1000))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    dataToDecrypt = br.ReadBytes((int)(fs.Length));
                }
            }

            using (BinaryReader br2 = new BinaryReader(InternalDecrypt(dataToDecrypt, password)))
            { 
                int lengs = (int)(dataToDecrypt.Length);  // br.BaseStream.Length   was there...
                decryptedData = br2.ReadBytes(lengs);
            }

            using (FileStream fs_dest = new FileStream(DecryptFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1000))
            {
                using (BinaryWriter bw = new BinaryWriter(fs_dest))
                {
                    bw.Write(decryptedData);
                } 
            }
         }           
      

        static CryptoStream InternalDecrypt(byte[] data, string password)
        {
            CryptAlgInit();
           
			//Key
			byte[] key = (new PasswordDeriveBytes(password, null)).GetBytes(sa.KeySize/8);
			//initialization vector:
			byte[] bytes = SHA256.Create().ComputeHash(new MemoryStream((new PasswordDeriveBytes(Reverse(password), null)).GetBytes(sa.KeySize/8)));
			byte[] iv = new byte[sa.KeySize/8];
			Array.Copy(bytes, iv, iv.Length);
			
//			Console.WriteLine("key.Length"+key.Length+", iv.Length"+iv.Length);	//this available, if compilation without -target:winexe

//            ICryptoTransform ct = sa.CreateDecryptor(
//                (new PasswordDeriveBytes(password, null)).GetBytes(16), initVector);
            ICryptoTransform ct = sa.CreateDecryptor(key, iv);

            MemoryStream ms = new MemoryStream(data);
            return new CryptoStream(ms, ct, CryptoStreamMode.Read);
        }
    }
}
