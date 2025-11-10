namespace Chat.Client
{
    partial class FormLogin
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
            panelLogin = new Panel();
            lblUserName1 = new Label();
            lblPort1 = new Label();
            lblHost1 = new Label();
            btnConnect = new Button();
            txtUserName = new TextBox();
            lblUserName = new Label();
            txtPort = new TextBox();
            lblPort = new Label();
            txtHost = new TextBox();
            lblHost = new Label();
            lblLogin = new Label();
            panelLogin.SuspendLayout();
            SuspendLayout();
            // 
            // panelLogin
            // 
            panelLogin.BackColor = SystemColors.Control;
            panelLogin.Controls.Add(lblUserName1);
            panelLogin.Controls.Add(lblPort1);
            panelLogin.Controls.Add(lblHost1);
            panelLogin.Controls.Add(btnConnect);
            panelLogin.Controls.Add(txtUserName);
            panelLogin.Controls.Add(lblUserName);
            panelLogin.Controls.Add(txtPort);
            panelLogin.Controls.Add(lblPort);
            panelLogin.Controls.Add(txtHost);
            panelLogin.Controls.Add(lblHost);
            panelLogin.Dock = DockStyle.Bottom;
            panelLogin.Location = new Point(0, 134);
            panelLogin.Margin = new Padding(3, 4, 3, 4);
            panelLogin.Name = "panelLogin";
            panelLogin.Size = new Size(800, 428);
            panelLogin.TabIndex = 0;
            panelLogin.Paint += panelLogin_Paint;
            // 
            // lblUserName1
            // 
            lblUserName1.Location = new Point(613, 185);
            lblUserName1.Name = "lblUserName1";
            lblUserName1.Size = new Size(139, 48);
            lblUserName1.TabIndex = 4;
            lblUserName1.Text = "Hãy nhập tên của bạn (tối đa 15 ký tự)";
            // 
            // lblPort1
            // 
            lblPort1.AutoSize = true;
            lblPort1.Location = new Point(613, 139);
            lblPort1.Name = "lblPort1";
            lblPort1.Size = new Size(68, 20);
            lblPort1.TabIndex = 4;
            lblPort1.Text = "VD: 8080";
            // 
            // lblHost1
            // 
            lblHost1.AutoSize = true;
            lblHost1.Location = new Point(613, 88);
            lblHost1.Name = "lblHost1";
            lblHost1.Size = new Size(109, 20);
            lblHost1.TabIndex = 4;
            lblHost1.Text = "VD: 192.168.3.2";
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.SpringGreen;
            btnConnect.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConnect.Location = new Point(325, 274);
            btnConnect.Margin = new Padding(3, 4, 3, 4);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(204, 49);
            btnConnect.TabIndex = 3;
            btnConnect.Text = "CONNECT";
            btnConnect.UseVisualStyleBackColor = false;
            // 
            // txtUserName
            // 
            txtUserName.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtUserName.Location = new Point(325, 185);
            txtUserName.Margin = new Padding(3, 4, 3, 4);
            txtUserName.Name = "txtUserName";
            txtUserName.Size = new Size(273, 30);
            txtUserName.TabIndex = 2;
            // 
            // lblUserName
            // 
            lblUserName.AutoSize = true;
            lblUserName.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblUserName.Location = new Point(118, 194);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new Size(145, 25);
            lblUserName.TabIndex = 0;
            lblUserName.Text = "USER NAME:";
            lblUserName.Click += label2_Click;
            // 
            // txtPort
            // 
            txtPort.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPort.Location = new Point(325, 130);
            txtPort.Margin = new Padding(3, 4, 3, 4);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(273, 30);
            txtPort.TabIndex = 1;
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblPort.Location = new Point(118, 139);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(78, 25);
            lblPort.TabIndex = 0;
            lblPort.Text = "PORT:";
            lblPort.Click += label2_Click;
            // 
            // txtHost
            // 
            txtHost.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtHost.Location = new Point(325, 79);
            txtHost.Margin = new Padding(3, 4, 3, 4);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(273, 30);
            txtHost.TabIndex = 0;
            txtHost.TextChanged += txtHost_TextChanged;
            // 
            // lblHost
            // 
            lblHost.AutoSize = true;
            lblHost.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHost.Location = new Point(118, 88);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(80, 25);
            lblHost.TabIndex = 0;
            lblHost.Text = "HOST:";
            lblHost.Click += label2_Click;
            // 
            // lblLogin
            // 
            lblLogin.Font = new Font("Microsoft Sans Serif", 19.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblLogin.ForeColor = SystemColors.Control;
            lblLogin.Location = new Point(0, -1);
            lblLogin.Name = "lblLogin";
            lblLogin.Size = new Size(800, 132);
            lblLogin.TabIndex = 1;
            lblLogin.Text = "LOGIN - CHATGUI";
            lblLogin.TextAlign = ContentAlignment.MiddleCenter;
            lblLogin.Click += lblLogin_Click;
            // 
            // FormLogin
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Navy;
            ClientSize = new Size(800, 562);
            Controls.Add(lblLogin);
            Controls.Add(panelLogin);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormLogin";
            Text = "Login";
            Load += FormLogin_Load;
            panelLogin.ResumeLayout(false);
            panelLogin.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLogin;
        private System.Windows.Forms.Label lblLogin;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblUserName1;
        private System.Windows.Forms.Label lblPort1;
        private System.Windows.Forms.Label lblHost1;
    }
}