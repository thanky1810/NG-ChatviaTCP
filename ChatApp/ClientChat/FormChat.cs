using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class Chat_TCP_Client : Form
    {
        // Thuộc tính để nhận dữ liệu từ FormLogin
        public string UserName { get; set; }

        public Chat_TCP_Client()
        {
            InitializeComponent();
            this.Load += Chat_TCP_Client_Load;
        }

        private void Chat_TCP_Client_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
                this.Text = $"Chat - {UserName}";
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }

        private void Connect_Click(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void btnLeave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Leave Room functionality is not implemented yet.",
                "Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void txtMessageInput_TextChanged(object sender, EventArgs e) { }

        private void pnlMessageList_Paint(object sender, PaintEventArgs e) { }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessInput.Text))
                return;

            string senderName = string.IsNullOrWhiteSpace(this.UserName) ? "Linh" : this.UserName;
            string time = DateTime.Now.ToString("HH:mm:ss");
            string message = txtMessInput.Text.Trim();

            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = $" {time} - {senderName}: {message} ";
            lbl.Location = new Point(10, pnlMessList.Controls.Count * 25);

            pnlMessList.Controls.Add(lbl);
            pnlMessList.ScrollControlIntoView(lbl);
            txtMessInput.Clear();
        }

        private void pnlChatFame_Paint(object sender, PaintEventArgs e) { }

        private void pnlChatHeader_Paint(object sender, PaintEventArgs e) { }

        private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e) { }

        private void Chat_TCP_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to leave the chat?",
                "Confirm Leave",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            e.Cancel = (result != DialogResult.Yes);
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Join Room functionality is not implemented yet.",
                "Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Create Room functionality is not implemented yet.");
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Log Out functionality is not implemented yet.");
        }
    }
}
