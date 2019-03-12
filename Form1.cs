using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace FileCryptWinApp
{
    public partial class Form1 : Form
    {
        public Form1() // constructor
        {
            InitializeComponent();
        }

        string sourceFileName;
        string destFileName;
        string pwd;
        public static CipherMode cipherMode = CipherMode.CBC; // default value
        public static int keySize = 128;    // default value

        private void PasswordInput()
        {
            pwd = textBoxPWD.Text;
            if (!Regex.IsMatch(pwd, @"([a-zA-Z0-9]){8,}$"))
                MessageBox.Show("Use at least 8 Latin letters or digits", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else label1.Text = "Pwd accepted";
        }
        
   
        private void btnDoEncryption_Click(object sender, EventArgs e)
        {
            PasswordInput();
            destFileName = sourceFileName + ".aes";
            if (File.Exists(sourceFileName)) { 
            FCrypt.Encrypt(sourceFileName, pwd, destFileName);
            }
            else MessageBox.Show("Choose file", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnDoDecryption_Click(object sender, EventArgs e)
        {
            PasswordInput();

            if (File.Exists(sourceFileName)) { 
                if (sourceFileName.EndsWith(".aes") )
            {   
                destFileName = sourceFileName.Remove(sourceFileName.Length - 4);
                    try { 
                    FCrypt.Decrypt(sourceFileName, pwd, destFileName);
                    }
                    catch
                    { MessageBox.Show("Wrong password", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
                else MessageBox.Show("Choose *.aes file", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else MessageBox.Show("Please choose file to decrypt", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void cBoxChainMode_SelectedIndexChanged(object sender, EventArgs e)
        {

            switch (cBoxChainMode.SelectedIndex)
            { 
                case 1:
                cipherMode = CipherMode.CBC;
                break;
                case 2:
                    cipherMode = CipherMode.CFB;
                    break;
                case 3:
                    cipherMode = CipherMode.CTS;
                    break;
                case 4:
                    cipherMode = CipherMode.ECB;
                    break;
                case 5:
                    cipherMode = CipherMode.OFB;
                    break;
                default:
                    cipherMode = CipherMode.CBC;
                    break;
            }
            }

        private void cBoxKeySize_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cBoxKeySize.SelectedIndex)
            {
                case 1:
                    keySize = 128;
                    break;
                case 2:
                    keySize = 192;
                    break;
                case 3:
                    keySize = 256;
                    break;
                default:
                    keySize = 128;
                    break;
            }
        }

        private void btnOpenForEncrypt_Click(object sender, EventArgs e)
        {
			try
			{
				OpenFileDialog openFileDialogEncr = new OpenFileDialog();
				if (openFileDialogEncr.ShowDialog() == DialogResult.OK)
				{
					sourceFileName = openFileDialogEncr.FileName;
					System.IO.StreamReader reader = System.IO.File.OpenText(openFileDialogEncr.FileName);
					string s = reader.ReadToEnd();
					reader.Close();
					lblFileToEncName.Text = "File is chosen!";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error:" + ex.Message);
			}
		}
    }
}
