namespace ClientChat
{
    partial class Chat_TCP_Client
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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlSideba = new System.Windows.Forms.TabControl();
            this.tbUser = new System.Windows.Forms.TabPage();
            this.lbUsers = new System.Windows.Forms.ListBox();
            this.tpRoom = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnJ = new System.Windows.Forms.Button();
            this.lbRooms = new System.Windows.Forms.ListBox();
            this.pnlChatFame = new System.Windows.Forms.Panel();
            this.pnlChatHeader = new System.Windows.Forms.Panel();
            this.lbNameRoom = new System.Windows.Forms.Label();
            this.btnLeave = new System.Windows.Forms.Button();
            this.pnlMessageList = new System.Windows.Forms.Panel();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessageInput = new System.Windows.Forms.TextBox();
            this.btnLogOut = new System.Windows.Forms.Button();
            this.pnlSidebar.SuspendLayout();
            this.pnlSideba.SuspendLayout();
            this.tbUser.SuspendLayout();
            this.tpRoom.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pnlChatFame.SuspendLayout();
            this.pnlChatHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pnlSidebar.Controls.Add(this.pnlSideba);
            this.pnlSidebar.Location = new System.Drawing.Point(12, 21);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(215, 499);
            this.pnlSidebar.TabIndex = 0;
            this.pnlSidebar.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // pnlSideba
            // 
            this.pnlSideba.Controls.Add(this.tbUser);
            this.pnlSideba.Controls.Add(this.tpRoom);
            this.pnlSideba.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSideba.Font = new System.Drawing.Font("Microsoft Tai Le", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlSideba.Location = new System.Drawing.Point(0, 0);
            this.pnlSideba.Name = "pnlSideba";
            this.pnlSideba.SelectedIndex = 0;
            this.pnlSideba.Size = new System.Drawing.Size(215, 499);
            this.pnlSideba.TabIndex = 0;
            this.pnlSideba.Tag = "";
            // 
            // tbUser
            // 
            this.tbUser.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.tbUser.Controls.Add(this.lbUsers);
            this.tbUser.Location = new System.Drawing.Point(4, 32);
            this.tbUser.Name = "tbUser";
            this.tbUser.Padding = new System.Windows.Forms.Padding(3);
            this.tbUser.Size = new System.Drawing.Size(207, 463);
            this.tbUser.TabIndex = 0;
            this.tbUser.Text = "Users";
            // 
            // lbUsers
            // 
            this.lbUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbUsers.FormattingEnabled = true;
            this.lbUsers.ItemHeight = 23;
            this.lbUsers.Location = new System.Drawing.Point(3, 3);
            this.lbUsers.Name = "lbUsers";
            this.lbUsers.Size = new System.Drawing.Size(201, 457);
            this.lbUsers.TabIndex = 0;
            // 
            // tpRoom
            // 
            this.tpRoom.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.tpRoom.Controls.Add(this.panel1);
            this.tpRoom.Controls.Add(this.lbRooms);
            this.tpRoom.Location = new System.Drawing.Point(4, 32);
            this.tpRoom.Name = "tpRoom";
            this.tpRoom.Padding = new System.Windows.Forms.Padding(3);
            this.tpRoom.Size = new System.Drawing.Size(207, 463);
            this.tpRoom.TabIndex = 1;
            this.tpRoom.Text = "Rooms";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCreate);
            this.panel1.Controls.Add(this.btnJ);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 326);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(201, 134);
            this.panel1.TabIndex = 1;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(50, 76);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(96, 41);
            this.btnCreate.TabIndex = 2;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            // 
            // btnJ
            // 
            this.btnJ.Location = new System.Drawing.Point(50, 19);
            this.btnJ.Name = "btnJ";
            this.btnJ.Size = new System.Drawing.Size(96, 41);
            this.btnJ.TabIndex = 1;
            this.btnJ.Text = "Join";
            this.btnJ.UseVisualStyleBackColor = true;
            // 
            // lbRooms
            // 
            this.lbRooms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRooms.FormattingEnabled = true;
            this.lbRooms.ItemHeight = 23;
            this.lbRooms.Location = new System.Drawing.Point(3, 3);
            this.lbRooms.Name = "lbRooms";
            this.lbRooms.Size = new System.Drawing.Size(201, 457);
            this.lbRooms.TabIndex = 0;
            // 
            // pnlChatFame
            // 
            this.pnlChatFame.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.pnlChatFame.Controls.Add(this.pnlChatHeader);
            this.pnlChatFame.Controls.Add(this.pnlMessageList);
            this.pnlChatFame.Controls.Add(this.btnSend);
            this.pnlChatFame.Controls.Add(this.txtMessageInput);
            this.pnlChatFame.Location = new System.Drawing.Point(247, 21);
            this.pnlChatFame.Name = "pnlChatFame";
            this.pnlChatFame.Size = new System.Drawing.Size(758, 499);
            this.pnlChatFame.TabIndex = 1;
            this.pnlChatFame.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlChatFame_Paint);
            // 
            // pnlChatHeader
            // 
            this.pnlChatHeader.BackColor = System.Drawing.SystemColors.HotTrack;
            this.pnlChatHeader.Controls.Add(this.lbNameRoom);
            this.pnlChatHeader.Controls.Add(this.btnLeave);
            this.pnlChatHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlChatHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlChatHeader.Name = "pnlChatHeader";
            this.pnlChatHeader.Size = new System.Drawing.Size(758, 57);
            this.pnlChatHeader.TabIndex = 3;
            this.pnlChatHeader.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlChatHeader_Paint);
            // 
            // lbNameRoom
            // 
            this.lbNameRoom.BackColor = System.Drawing.SystemColors.HighlightText;
            this.lbNameRoom.Location = new System.Drawing.Point(16, 13);
            this.lbNameRoom.Name = "lbNameRoom";
            this.lbNameRoom.Size = new System.Drawing.Size(195, 30);
            this.lbNameRoom.TabIndex = 1;
            this.lbNameRoom.Text = "NameRoom";
            this.lbNameRoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLeave
            // 
            this.btnLeave.BackColor = System.Drawing.Color.DimGray;
            this.btnLeave.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnLeave.Location = new System.Drawing.Point(645, 9);
            this.btnLeave.Name = "btnLeave";
            this.btnLeave.Size = new System.Drawing.Size(75, 39);
            this.btnLeave.TabIndex = 0;
            this.btnLeave.Text = "Leave";
            this.btnLeave.UseVisualStyleBackColor = false;
            this.btnLeave.Click += new System.EventHandler(this.btnLeave_Click);
            // 
            // pnlMessageList
            // 
            this.pnlMessageList.AutoScroll = true;
            this.pnlMessageList.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnlMessageList.Location = new System.Drawing.Point(20, 72);
            this.pnlMessageList.Name = "pnlMessageList";
            this.pnlMessageList.Size = new System.Drawing.Size(700, 332);
            this.pnlMessageList.TabIndex = 2;
            this.pnlMessageList.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlMessageList_Paint);
            // 
            // btnSend
            // 
            this.btnSend.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnSend.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSend.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.btnSend.Location = new System.Drawing.Point(645, 424);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 60);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = false;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessageInput
            // 
            this.txtMessageInput.Location = new System.Drawing.Point(20, 424);
            this.txtMessageInput.Multiline = true;
            this.txtMessageInput.Name = "txtMessageInput";
            this.txtMessageInput.Size = new System.Drawing.Size(594, 60);
            this.txtMessageInput.TabIndex = 0;
            this.txtMessageInput.TextChanged += new System.EventHandler(this.txtMessageInput_TextChanged);
            // 
            // btnLogOut
            // 
            this.btnLogOut.Location = new System.Drawing.Point(858, 528);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(147, 44);
            this.btnLogOut.TabIndex = 2;
            this.btnLogOut.Text = "LogOut";
            this.btnLogOut.UseVisualStyleBackColor = true;
            // 
            // Chat_TCP_Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(1026, 584);
            this.Controls.Add(this.btnLogOut);
            this.Controls.Add(this.pnlChatFame);
            this.Controls.Add(this.pnlSidebar);
            this.Name = "Chat_TCP_Client";
            this.Text = "Chat Client";
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSideba.ResumeLayout(false);
            this.tbUser.ResumeLayout(false);
            this.tpRoom.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pnlChatFame.ResumeLayout(false);
            this.pnlChatFame.PerformLayout();
            this.pnlChatHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Panel pnlChatFame;
        private System.Windows.Forms.TextBox txtMessageInput;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TabControl pnlSideba;
        private System.Windows.Forms.TabPage tbUser;
        private System.Windows.Forms.TabPage tpRoom;
        private System.Windows.Forms.ListBox lbUsers;
        private System.Windows.Forms.ListBox lbRooms;
        private System.Windows.Forms.Panel pnlChatHeader;
        private System.Windows.Forms.Button btnLeave;
        private System.Windows.Forms.Button btnLogOut;
        private System.Windows.Forms.Label lbNameRoom;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnJ;
        private System.Windows.Forms.Panel pnlMessageList;
    }
}

