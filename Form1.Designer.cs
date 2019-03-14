using System.IO;        //using File.Exists(pathway)

namespace FileCryptWinApp
{
        partial class Form1
        {
                /// <summary>
                /// Required designer variable.
                /// </summary>
                private System.ComponentModel.IContainer components = null;

                /// <summary>
                /// Clean up any resources being used.
                /// </summary>
                /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
                protected override void Dispose(bool disposing)
                {
                        if (disposing && (components != null))
                        {
                                components.Dispose();
                        }
                        base.Dispose(disposing);
                }

                #region Windows Form Designer generated code

                /// <summary>
                /// Required method for Designer support - do not modify
                /// the contents of this method with the code editor.
                /// </summary>
                private void InitializeComponent()
                {
                        this.btnOpenForEncrypt = new System.Windows.Forms.Button();
                        this.btnDoEncryption = new System.Windows.Forms.Button();
                        this.lblFileToEncName = new System.Windows.Forms.Label();
                        this.textBoxPWD = new System.Windows.Forms.TextBox();
                        this.groupBox1 = new System.Windows.Forms.GroupBox();
                        this.cBoxKeySize = new System.Windows.Forms.ComboBox();
                        this.cBoxChainMode = new System.Windows.Forms.ComboBox();
                        this.label3 = new System.Windows.Forms.Label();
                        this.label2 = new System.Windows.Forms.Label();
                        this.label1 = new System.Windows.Forms.Label();
                        this.btnDoDecryption = new System.Windows.Forms.Button();
                        this.openFileDialogEncr = new System.Windows.Forms.OpenFileDialog();
                        this.openFileDialogEncr.Filter = "aes files (*.aes)|*.aes|All files (*.*)|*.*";
                        this.groupBox1.SuspendLayout();

                        this.statusStrip1 = new System.Windows.Forms.StatusStrip();
                        this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
                        this.linklabel = new System.Windows.Forms.LinkLabel();
                        this.statusStrip1.SuspendLayout();
                        this.SuspendLayout();

                        // 
                        // btnOpenForEncrypt
                        // 
                        this.btnOpenForEncrypt.Location = new System.Drawing.Point(175, 24);
                        this.btnOpenForEncrypt.Name = "btnOpenForEncrypt";
                        this.btnOpenForEncrypt.Size = new System.Drawing.Size(300, 30);
                        this.btnOpenForEncrypt.TabIndex = 0;
                        this.btnOpenForEncrypt.Text = "Choose the file for encryption/decryption...";
                        this.btnOpenForEncrypt.UseVisualStyleBackColor = true;
                        this.btnOpenForEncrypt.Click += new System.EventHandler(this.btnOpenForEncrypt_Click);
                        // 
                        // btnDoEncryption
                        // 
                        this.btnDoEncryption.Location = new System.Drawing.Point(175, 110);
                        this.btnDoEncryption.Name = "btnDoEncryption";
                        this.btnDoEncryption.Size = new System.Drawing.Size(120, 30);
                        this.btnDoEncryption.TabIndex = 0;
                        this.btnDoEncryption.Text = "Encrypt!";
                        this.btnDoEncryption.UseVisualStyleBackColor = true;
                        this.btnDoEncryption.Click += new System.EventHandler(this.btnDoEncryption_Click);
                        // 
                        // btnDoDecryption
                        // 
                        this.btnDoDecryption.Location = new System.Drawing.Point(350, 110);
                        this.btnDoDecryption.Name = "btnDoDecryption";
                        this.btnDoDecryption.Size = new System.Drawing.Size(120, 30);
                        this.btnDoDecryption.TabIndex = 0;
                        this.btnDoDecryption.Text = "Decrypt!";
                        this.btnDoDecryption.UseVisualStyleBackColor = true;
                        this.btnDoDecryption.Click += new System.EventHandler(this.btnDoDecryption_Click);
                        // 
                        // lblFileToEncName
                        // 
                        this.lblFileToEncName.AutoSize = true;
                        this.lblFileToEncName.Location = new System.Drawing.Point(175, 75);
                        this.lblFileToEncName.Name = "lblFileToEncName";
                        this.lblFileToEncName.Size = new System.Drawing.Size(116, 17);
                        this.lblFileToEncName.TabIndex = 1;
                        this.lblFileToEncName.Text = "File not chosen...";
                        
                        // 
                        // statusStrip1
                        // 
                        this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        this.lblStatus});
                        this.statusStrip1.Location = new System.Drawing.Point(175, 165);
                        this.statusStrip1.Name = "statusStrip1";
                        this.statusStrip1.Size = new System.Drawing.Size(165, 22);
                        this.statusStrip1.TabIndex = 9;
                        this.statusStrip1.Text = "statusStrip1";
                        // 
                        // lblStatus
                        // 
                        this.lblStatus.Name = "lblStatus";
                        this.lblStatus.Size = new System.Drawing.Size(250, 200);
                        this.lblStatus.Text = "Ready...";
                        // 
                        // linklabel
                        // 
                        this.linklabel.AutoSize = true;
                        this.linklabel.Location = new System.Drawing.Point(400, 145);
                        this.linklabel.Name = "linklabel";
                        this.linklabel.Size = new System.Drawing.Size(300, 22);
                        this.linklabel.TabIndex = 10;
                        this.linklabel.TabStop = true;
                        this.linklabel.Text = "https://github.com/username1565/WinFormsCryptApp/releases";
                        this.linklabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linklabel_LinkClicked);

                        // 
                        // textBoxPWD
                        // 
                        this.textBoxPWD.Location = new System.Drawing.Point(131, 21);
                        this.textBoxPWD.Name = "textBoxPWD";
                        this.textBoxPWD.Size = new System.Drawing.Size(350, 22);
                        this.textBoxPWD.TabIndex = 2;
                        this.textBoxPWD.Text = ((File.Exists(this.encrypted_key_iv_file) && File.Exists(this.priv_file)) ? this.keyfiles_pass : this.default_password);
                        //this.textBoxPWD.Enabled = false;
                        
                        // 
                        // groupBox1
                        // 
                        this.groupBox1.Controls.Add(this.cBoxKeySize);
                        this.groupBox1.Controls.Add(this.cBoxChainMode);
                        this.groupBox1.Controls.Add(this.label3);
                        this.groupBox1.Controls.Add(this.label2);
                        this.groupBox1.Controls.Add(this.label1);

                        this.Controls.Add(this.linklabel);
                        this.Controls.Add(this.statusStrip1);
                        this.statusStrip1.ResumeLayout(false);
                        this.statusStrip1.PerformLayout();


                        this.groupBox1.Controls.Add(this.textBoxPWD);
                        this.groupBox1.Location = new System.Drawing.Point(625, 17);
                        this.groupBox1.Name = "groupBox1";
                        this.groupBox1.Size = new System.Drawing.Size(550, 125);
                        this.groupBox1.TabIndex = 3;
                        this.groupBox1.TabStop = false;
                        this.groupBox1.Text = "Encrypt / Decrypt parameters";
                        // 
                        // cBoxKeySize
                        // 
                        this.cBoxKeySize.FormattingEnabled = true;
                        this.cBoxKeySize.Items.AddRange(new object[] {
                        "Select KeySize",
                        "128",
                        "192",
                        "256"});
                        this.cBoxKeySize.Location = new System.Drawing.Point(131, 86);
                        this.cBoxKeySize.Name = "cBoxKeySize";
                        this.cBoxKeySize.Size = new System.Drawing.Size(220, 24);
                        this.cBoxKeySize.TabIndex = 5;
                        this.cBoxKeySize.Text = "Please Select an Item";
                        this.cBoxKeySize.SelectedIndex = 3;
                        this.cBoxKeySize.SelectedIndexChanged += new System.EventHandler(this.cBoxKeySize_SelectedIndexChanged);
                        // 
                        // cBoxChainMode
                        // 
                        this.cBoxChainMode.Items.AddRange(new object[] {
                        "Please select",
                        "ECB",
                        "CBC",
                        "CFB",
                        "CTS",
                        "OFB"});
                        this.cBoxChainMode.Location = new System.Drawing.Point(131, 54);
                        this.cBoxChainMode.Name = "cBoxChainMode";
                        this.cBoxChainMode.Size = new System.Drawing.Size(220, 24);
                        this.cBoxChainMode.TabIndex = 4;
                        this.cBoxChainMode.Text = "Please Select an Item";
                        this.cBoxChainMode.SelectedIndex = 2;
                        this.cBoxChainMode.SelectedIndexChanged += new System.EventHandler(this.cBoxChainMode_SelectedIndexChanged);
                        // 
                        // label3
                        // 
                        this.label3.AutoSize = true;
                        this.label3.Location = new System.Drawing.Point(60, 88);
                        this.label3.Name = "label3";
                        this.label3.Size = new System.Drawing.Size(65, 17);
                        this.label3.TabIndex = 3;
                        this.label3.Text = "Key size:";
                        // 
                        // label2
                        // 
                        this.label2.AutoSize = true;
                        this.label2.Location = new System.Drawing.Point(18, 57);
                        this.label2.Name = "label2";
                        this.label2.Size = new System.Drawing.Size(106, 17);
                        this.label2.TabIndex = 3;
                        this.label2.Text = "Chaining mode:";
                        // 
                        // label1
                        // 
                        this.label1.AutoSize = true;
                        this.label1.Location = new System.Drawing.Point(7, 25);
                        this.label1.Name = "label1";
                        this.label1.Size = new System.Drawing.Size(110, 17);
                        this.label1.TabIndex = 3;
                        this.label1.Text = "Enter password:";
                        // 
                        // openFileDialogEncr
                        // 
                        this.openFileDialogEncr.DefaultExt = "aes";
                        this.openFileDialogEncr.FileName = "Choose file to be encrypted / decrypted";
                        // 
                        // Form1
                        // 
                        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
                        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                        this.ClientSize = new System.Drawing.Size(1200, 200);
                        this.Controls.Add(this.groupBox1);
                        this.Controls.Add(this.lblFileToEncName);
                        this.Controls.Add(this.btnDoDecryption);
                        this.Controls.Add(this.btnDoEncryption);
                        this.Controls.Add(this.btnOpenForEncrypt);
                        this.Name = "Form1";
                        this.Text = "Encrypt / decrypt files with AES cipher";
                        this.groupBox1.ResumeLayout(false);
                        this.groupBox1.PerformLayout();
                        this.ResumeLayout(false);
                        this.PerformLayout();

                }

                #endregion

                private System.Windows.Forms.Button btnOpenForEncrypt;
                private System.Windows.Forms.Button btnDoEncryption;
                private System.Windows.Forms.Label lblFileToEncName;
                private System.Windows.Forms.TextBox textBoxPWD;
                private System.Windows.Forms.GroupBox groupBox1;
                private System.Windows.Forms.ComboBox cBoxChainMode;
                private System.Windows.Forms.Label label3;
                private System.Windows.Forms.Label label2;
                private System.Windows.Forms.Label label1;
                private System.Windows.Forms.Button btnDoDecryption;
                private System.Windows.Forms.ComboBox cBoxKeySize;
                private System.Windows.Forms.OpenFileDialog openFileDialogEncr;
                
                private System.Windows.Forms.StatusStrip statusStrip1;
                public System.Windows.Forms.ToolStripStatusLabel lblStatus;
                private System.Windows.Forms.LinkLabel linklabel;

        }
}

