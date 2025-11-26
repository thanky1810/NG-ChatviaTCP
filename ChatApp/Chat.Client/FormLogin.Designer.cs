// File: Chat.Client/FormLogin.Designer.cs
// (Người 6 - Cao Xuân Quyết: Thiết kế Giao diện Form Đăng nhập)
namespace Chat.Client
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelLogin = new System.Windows.Forms.Panel();
            this.lblUserName1 = new System.Windows.Forms.Label();
            this.lblPort1 = new System.Windows.Forms.Label();
            this.lblHost1 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button(); // (Người 6: Nút Kết nối)
            this.txtUserName = new System.Windows.Forms.TextBox(); // (Người 6: Ô nhập Tên)
            this.lblUserName = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox(); // (Người 6: Ô nhập Cổng)
            this.lblPort = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox(); // (Người 6: Ô nhập IP)
            this.lblHost = new System.Windows.Forms.Label();
            this.lblLogin = new System.Windows.Forms.Label();
            this.panelLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLogin
            // 
            this.panelLogin.BackColor = System.Drawing.SystemColors.Control;
            this.panelLogin.Controls.Add(this.lblUserName1);
            this.panelLogin.Controls.Add(this.lblPort1);
            this.panelLogin.Controls.Add(this.lblHost1);
            this.panelLogin.Controls.Add(this.btnConnect);
            this.panelLogin.Controls.Add(this.txtUserName);
            this.panelLogin.Controls.Add(this.lblUserName);
            this.panelLogin.Controls.Add(this.txtPort);
            this.panelLogin.Controls.Add(this.lblPort);
            this.panelLogin.Controls.Add(this.txtHost);
            this.panelLogin.Controls.Add(this.lblHost);
            this.panelLogin.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLogin.Location = new System.Drawing.Point(0, 134);
            this.panelLogin.Name = "panelLogin";
            this.panelLogin.Size = new System.Drawing.Size(800, 428);
            this.panelLogin.TabIndex = 0;
            // 
            // lblUserName1
            // 
            this.lblUserName1.Location = new System.Drawing.Point(613, 185);
            this.lblUserName1.Name = "lblUserName1";
            this.lblUserName1.Size = new System.Drawing.Size(139, 48);
            this.lblUserName1.TabIndex = 4;
            this.lblUserName1.Text = "Tên hiển thị (max 15 ký tự)";
            // 
            // lblPort1
            // 
            this.lblPort1.AutoSize = true;
            this.lblPort1.Location = new System.Drawing.Point(613, 139);
            this.lblPort1.Name = "lblPort1";
            this.lblPort1.Size = new System.Drawing.Size(68, 20);
            this.lblPort1.TabIndex = 4;
            this.lblPort1.Text = "VD: 8888";
            // 
            // lblHost1
            // 
            this.lblHost1.AutoSize = true;
            this.lblHost1.Location = new System.Drawing.Point(613, 88);
            this.lblHost1.Name = "lblHost1";
            this.lblHost1.Size = new System.Drawing.Size(109, 20);
            this.lblHost1.TabIndex = 4;
            this.lblHost1.Text = "VD: 127.0.0.1";
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.SpringGreen;
            this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btnConnect.Location = new System.Drawing.Point(325, 274);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(204, 49);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "CONNECT";
            this.btnConnect.UseVisualStyleBackColor = false;
            // 
            // txtUserName
            // 
            this.txtUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtUserName.Location = new System.Drawing.Point(325, 185);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(273, 30);
            this.txtUserName.TabIndex = 2;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.lblUserName.Location = new System.Drawing.Point(118, 194);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(145, 25);
            this.lblUserName.TabIndex = 0;
            this.lblUserName.Text = "USER NAME:";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtPort.Location = new System.Drawing.Point(325, 130);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(273, 30);
            this.txtPort.TabIndex = 1;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.lblPort.Location = new System.Drawing.Point(118, 139);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(78, 25);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "PORT:";
            // 
            // txtHost
            // 
            this.txtHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.txtHost.Location = new System.Drawing.Point(325, 79);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(273, 30);
            this.txtHost.TabIndex = 0;
            // 
            // lblHost
            // 
            this.lblHost.AutoSize = true;
            this.lblHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.lblHost.Location = new System.Drawing.Point(118, 88);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(80, 25);
            this.lblHost.TabIndex = 0;
            this.lblHost.Text = "HOST:";
            // 
            // lblLogin
            // 
            this.lblLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Bold);
            this.lblLogin.ForeColor = System.Drawing.SystemColors.Control;
            this.lblLogin.Location = new System.Drawing.Point(0, -1);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(800, 132);
            this.lblLogin.TabIndex = 1;
            this.lblLogin.Text = "LOGIN - CHATGUI";
            this.lblLogin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Navy;
            this.ClientSize = new System.Drawing.Size(800, 562);
            this.Controls.Add(this.lblLogin);
            this.Controls.Add(this.panelLogin);
            this.Name = "FormLogin";
            this.Text = "Login";
            this.panelLogin.ResumeLayout(false);
            this.panelLogin.PerformLayout();
            this.ResumeLayout(false);
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