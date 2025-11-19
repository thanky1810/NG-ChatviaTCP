// File: UI.Chat/FormChat.Designer.cs
// (Và Chat.Client/FormChat.Designer.cs)
namespace Chat.Client
{
    partial class Chat_TCP_Client
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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.tcontlSidebar_ = new System.Windows.Forms.TabControl();
            this.tpageUser = new System.Windows.Forms.TabPage();
            this.lboxUsers = new System.Windows.Forms.ListBox();
            this.tpRoom = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnJoin = new System.Windows.Forms.Button();
            this.lboxRooms = new System.Windows.Forms.ListBox();
            this.pnlFame = new System.Windows.Forms.Panel();
            this.rtbMessList = new System.Windows.Forms.RichTextBox();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblNameRoom = new System.Windows.Forms.Label();
            this.btnLeave = new System.Windows.Forms.Button();
            this.btnPing = new System.Windows.Forms.Button(); // ✅ THÊM NÚT PING
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessInput = new System.Windows.Forms.TextBox();
            this.btnLogOut = new System.Windows.Forms.Button();
            this.pnlSidebar.SuspendLayout();
            this.tcontlSidebar_.SuspendLayout();
            this.tpageUser.SuspendLayout();
            this.tpRoom.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlFame.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlSidebar.Controls.Add(this.tcontlSidebar_);
            this.pnlSidebar.Location = new System.Drawing.Point(11, 17);
            this.pnlSidebar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(191, 399);
            this.pnlSidebar.TabIndex = 0;
            this.pnlSidebar.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // tcontlSidebar_
            // 
            this.tcontlSidebar_.Controls.Add(this.tpageUser);
            this.tcontlSidebar_.Controls.Add(this.tpRoom);
            this.tcontlSidebar_.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcontlSidebar_.Font = new System.Drawing.Font("Microsoft Tai Le", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcontlSidebar_.Location = new System.Drawing.Point(0, 0);
            this.tcontlSidebar_.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tcontlSidebar_.Name = "tcontlSidebar_";
            this.tcontlSidebar_.SelectedIndex = 0;
            this.tcontlSidebar_.Size = new System.Drawing.Size(191, 399);
            this.tcontlSidebar_.TabIndex = 0;
            this.tcontlSidebar_.Tag = "";
            // 
            // tpageUser
            // 
            this.tpageUser.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.tpageUser.Controls.Add(this.lboxUsers);
            this.tpageUser.Location = new System.Drawing.Point(4, 28);
            this.tpageUser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpageUser.Name = "tpageUser";
            this.tpageUser.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpageUser.Size = new System.Drawing.Size(183, 367);
            this.tpageUser.TabIndex = 0;
            this.tpageUser.Text = "Users";
            // 
            // lboxUsers
            // 
            this.lboxUsers.FormattingEnabled = true;
            this.lboxUsers.ItemHeight = 19;
            this.lboxUsers.Location = new System.Drawing.Point(1, 0);
            this.lboxUsers.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lboxUsers.Name = "lboxUsers";
            this.lboxUsers.Size = new System.Drawing.Size(184, 365);
            this.lboxUsers.TabIndex = 0;
            this.lboxUsers.SelectedIndexChanged += new System.EventHandler(this.lboxUsers_SelectedIndexChanged);
            // 
            // tpRoom
            // 
            this.tpRoom.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.tpRoom.Controls.Add(this.panel1);
            this.tpRoom.Controls.Add(this.lboxRooms);
            this.tpRoom.Location = new System.Drawing.Point(4, 28);
            this.tpRoom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpRoom.Name = "tpRoom";
            this.tpRoom.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tpRoom.Size = new System.Drawing.Size(183, 367);
            this.tpRoom.TabIndex = 1;
            this.tpRoom.Text = "Rooms";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCreate);
            this.panel1.Controls.Add(this.btnJoin);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 263);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(177, 102);
            this.panel1.TabIndex = 1;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(44, 61);
            this.btnCreate.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(85, 33);
            this.btnCreate.TabIndex = 2;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnJoin
            // 
            this.btnJoin.Location = new System.Drawing.Point(44, 15);
            this.btnJoin.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(85, 33);
            this.btnJoin.TabIndex = 1;
            this.btnJoin.Text = "Join";
            this.btnJoin.UseVisualStyleBackColor = true;
            this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
            // 
            // lboxRooms
            // 
            this.lboxRooms.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lboxRooms.FormattingEnabled = true;
            this.lboxRooms.HorizontalScrollbar = true;
            this.lboxRooms.IntegralHeight = false;
            this.lboxRooms.ItemHeight = 20;
            this.lboxRooms.Location = new System.Drawing.Point(3, 2);
            this.lboxRooms.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lboxRooms.Name = "lboxRooms";
            this.lboxRooms.Size = new System.Drawing.Size(179, 262);
            this.lboxRooms.TabIndex = 0;
            this.lboxRooms.SelectedIndexChanged += new System.EventHandler(this.lboxRooms_SelectedIndexChanged);
            // 
            // pnlFame
            // 
            this.pnlFame.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.pnlFame.Controls.Add(this.rtbMessList);
            this.pnlFame.Controls.Add(this.pnlHeader);
            this.pnlFame.Controls.Add(this.btnSend);
            this.pnlFame.Controls.Add(this.txtMessInput);
            this.pnlFame.Location = new System.Drawing.Point(220, 17);
            this.pnlFame.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlFame.Name = "pnlFame";
            this.pnlFame.Size = new System.Drawing.Size(674, 399);
            this.pnlFame.TabIndex = 1;
            this.pnlFame.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlChatFame_Paint);
            // 
            // rtbMessList
            // 
            this.rtbMessList.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.rtbMessList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbMessList.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbMessList.Location = new System.Drawing.Point(18, 50);
            this.rtbMessList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rtbMessList.Name = "rtbMessList";
            this.rtbMessList.ReadOnly = true;
            this.rtbMessList.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbMessList.Size = new System.Drawing.Size(623, 276);
            this.rtbMessList.TabIndex = 4;
            this.rtbMessList.Text = "";
            this.rtbMessList.TextChanged += new System.EventHandler(this.rtbMessList_TextChanged);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.SystemColors.HotTrack;
            this.pnlHeader.Controls.Add(this.lblNameRoom);
            this.pnlHeader.Controls.Add(this.btnLeave);
            this.pnlHeader.Controls.Add(this.btnPing); // ✅ THÊM
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(674, 46);
            this.pnlHeader.TabIndex = 3;
            this.pnlHeader.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlChatHeader_Paint);
            // 
            // lblNameRoom
            // 
            this.lblNameRoom.BackColor = System.Drawing.SystemColors.HighlightText;
            this.lblNameRoom.Location = new System.Drawing.Point(14, 10);
            this.lblNameRoom.Name = "lblNameRoom";
            this.lblNameRoom.Size = new System.Drawing.Size(173, 24);
            this.lblNameRoom.TabIndex = 1;
            this.lblNameRoom.Text = "NameRoom";
            this.lblNameRoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLeave
            // 
            this.btnLeave.BackColor = System.Drawing.Color.DimGray;
            this.btnLeave.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnLeave.Location = new System.Drawing.Point(573, 7);
            this.btnLeave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLeave.Name = "btnLeave";
            this.btnLeave.Size = new System.Drawing.Size(67, 31);
            this.btnLeave.TabIndex = 0;
            this.btnLeave.Text = "Leave";
            this.btnLeave.UseVisualStyleBackColor = false;
            this.btnLeave.Click += new System.EventHandler(this.btnLeave_Click);
            // 
            // btnPing (Cấu hình nút Ping)
            // 
            this.btnPing.BackColor = System.Drawing.Color.Orange;
            this.btnPing.ForeColor = System.Drawing.Color.White;
            this.btnPing.Location = new System.Drawing.Point(490, 7); // Cạnh nút Leave
            this.btnPing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPing.Name = "btnPing";
            this.btnPing.Size = new System.Drawing.Size(67, 31);
            this.btnPing.TabIndex = 5;
            this.btnPing.Text = "Ping";
            this.btnPing.UseVisualStyleBackColor = false;
            this.btnPing.Click += new System.EventHandler(this.btnPing_Click);
            // 
            // btnSend
            // 
            this.btnSend.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnSend.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSend.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.btnSend.Location = new System.Drawing.Point(573, 339);
            this.btnSend.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(67, 48);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessInput
            // 
            this.txtMessInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessInput.Location = new System.Drawing.Point(18, 339);
            this.txtMessInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMessInput.Multiline = true;
            this.txtMessInput.Name = "txtMessInput";
            this.txtMessInput.Size = new System.Drawing.Size(528, 49);
            this.txtMessInput.TabIndex = 0;
            this.txtMessInput.TextChanged += new System.EventHandler(this.txtMessageInput_TextChanged);
            // 
            // btnLogOut
            // 
            this.btnLogOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogOut.ForeColor = System.Drawing.Color.Red;
            this.btnLogOut.Location = new System.Drawing.Point(763, 422);
            this.btnLogOut.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(131, 35);
            this.btnLogOut.TabIndex = 2;
            this.btnLogOut.Text = "LogOut";
            this.btnLogOut.UseVisualStyleBackColor = true;
            this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
            // 
            // Chat_TCP_Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(912, 467);
            this.Controls.Add(this.btnLogOut);
            this.Controls.Add(this.pnlFame);
            this.Controls.Add(this.pnlSidebar);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Chat_TCP_Client";
            this.Text = "Chat Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Chat_TCP_Client_FormClosing);
            this.pnlSidebar.ResumeLayout(false);
            this.tcontlSidebar_.ResumeLayout(false);
            this.tpageUser.ResumeLayout(false);
            this.tpRoom.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pnlFame.ResumeLayout(false);
            this.pnlFame.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlFame;
        private System.Windows.Forms.TextBox txtMessInput;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TabControl tcontlSidebar_;
        private System.Windows.Forms.TabPage tpageUser;
        private System.Windows.Forms.TabPage tpRoom;
        private System.Windows.Forms.ListBox lboxUsers;
        private System.Windows.Forms.ListBox lboxRooms;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Button btnLeave;
        private System.Windows.Forms.Button btnLogOut;
        private System.Windows.Forms.Label lblNameRoom;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.RichTextBox rtbMessList;
        private System.Windows.Forms.Button btnPing; // ✅ BIẾN NÀY
    }
}