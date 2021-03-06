﻿using System;
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
                        this.TopMost = true;                           //show before all windows
                        InitializeComponent();
                }

                string sourceFileName;
                string destFileName;
                string pwd;
                public static CipherMode cipherMode = CipherMode.CBC;  // default value
                public static int keySize = 256;                       // default value
                public const int blockSize = 128;
                

                private void PasswordInput()
                {
                        pwd = textBoxPWD.Text;
                        if (!Regex.IsMatch(pwd, @"([-_a-zA-Z0-9]){8,}$"))
                                MessageBox.Show("Use at least 8 Latin letters or digits and \"-_\"", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else label1.Text = "Pwd accepted";
                }
                
                private void linklabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
                {
                        SetStatus("Commits and modifications of this OPEN SOURCE CODE - you can see here! ");
                        System.Diagnostics.Process.Start("https://github.com/username1565/WinFormsCryptApp/network");
                }

                private void btnDoEncryption_Click(object sender, EventArgs e)
                {
                        PasswordInput();
                        destFileName = sourceFileName + ".aes";
                        if (File.Exists(sourceFileName)) {
                                //FCrypt.Encrypt(sourceFileName, pwd, destFileName);
                                Encrypt(sourceFileName, pwd, destFileName);
                                return;
                        }
                        else MessageBox.Show("Choose file", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        try
                        {
                                SetStatus("Choose the file for encryption! ");
                                OpenFileDialog openFileDialogEncr = new OpenFileDialog();
                                if (openFileDialogEncr.ShowDialog() == DialogResult.OK)
                                {
                                        sourceFileName = openFileDialogEncr.FileName;
                                        System.IO.StreamReader reader = System.IO.File.OpenText(openFileDialogEncr.FileName);
                                        string s = reader.ReadToEnd();
                                        reader.Close();
                                        //lblFileToEncName.Text = "File is chosen! ";
                                }
                                SetStatus("File for encryption selected! ");
                        }
                        catch (Exception ex)
                        {
                                MessageBox.Show("Error:" + ex.Message+"! ");
                        }                
                }

                private void btnDoDecryption_Click(object sender, EventArgs e)
                {
                        PasswordInput();

                        if (File.Exists(sourceFileName)) { 
                                if (sourceFileName.EndsWith(".aes") )
                                {   
                                        SetStatus("AES file selected! You can try to decrypt it! ");
                                        destFileName = sourceFileName.Remove(sourceFileName.Length - 4);
                                                try { 
                                                        //FCrypt.Decrypt(sourceFileName, pwd, destFileName);
                                                        Decrypt(sourceFileName, pwd, destFileName);
                                                        return;
                                                }
                                                catch
                                                        { MessageBox.Show("Wrong password", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                                }
                                else MessageBox.Show("Choose *.aes file", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else {
                                MessageBox.Show("Please choose file to decrypt", "FileCryptWinApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        try
                        {
                                SetStatus("Choose the file for encryption/decryption! For decryption - choose *.aes! ");
                                OpenFileDialog openFileDialogEncr = new OpenFileDialog();
                                openFileDialogEncr.Filter = "AES Encrypted Files(*.aes)|*.aes";
                                if (openFileDialogEncr.ShowDialog() == DialogResult.OK)
                                {
                                        sourceFileName = openFileDialogEncr.FileName;
                                        System.IO.StreamReader reader = System.IO.File.OpenText(openFileDialogEncr.FileName);
                                        string s = reader.ReadToEnd();
                                        reader.Close();
                                        //lblFileToEncName.Text = "File is chosen! ";
                                }
                                SetStatus("AES file selected! You can try to decrypt it! ");
                        }
                        catch (Exception ex)
                        {
                                MessageBox.Show("Error:" + ex.Message+"! ");
                        }                        
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
                                        keySize = 256;
                                        break;
                        }
                }

                private void btnOpenForEncrypt_Click(object sender, EventArgs e)
                {
                        try
                        {
                                SetStatus("Choose the file for encryption/decryption! For decryption - choose *.aes! ");
                                OpenFileDialog openFileDialogEncr = new OpenFileDialog();
                                if (openFileDialogEncr.ShowDialog() == DialogResult.OK)
                                {
                                        sourceFileName = openFileDialogEncr.FileName;
                                        System.IO.StreamReader reader = System.IO.File.OpenText(openFileDialogEncr.FileName);
                                        string s = reader.ReadToEnd();
                                        reader.Close();
                                        lblFileToEncName.Text = "File is chosen! ";
                                }
                                SetStatus("File selected! ");
                        }
                        catch (Exception ex)
                        {
                                MessageBox.Show("Error:" + ex.Message+"! ");
                        }
                }
        }
}
