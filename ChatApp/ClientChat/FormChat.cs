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

<<<<<<< HEAD
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = $" {time} - {senderName}: {message} ";
            lbl.Location = new Point(10, pnlMessList.Controls.Count * 25);

            pnlMessList.Controls.Add(lbl);
            pnlMessList.ScrollControlIntoView(lbl);
=======
            AppendChatMessage(time, senderName, message, true); // true = tin của mình

>>>>>>> 0b20eb3d9a3d89ecd1b97566b2407d1d8728a5a9
            txtMessInput.Clear();
        }

        private void pnlChatFame_Paint(object sender, PaintEventArgs e) { }

        private void pnlChatHeader_Paint(object sender, PaintEventArgs e) { }

        private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e) { }

        private void Chat_TCP_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
<<<<<<< HEAD
                "Are you sure you want to leave the chat?",
                "Confirm Leave",
                MessageBoxButtons.YesNo,
=======
                "Are you sure you want to close the chat?", 
                "Confirm Leave", 
                MessageBoxButtons.YesNo, 
>>>>>>> 0b20eb3d9a3d89ecd1b97566b2407d1d8728a5a9
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
            using (var dlg = new FrmCreate())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var roomName = dlg.RoomName; // <-- Đọc từ property

                    if (!string.IsNullOrEmpty(roomName))
                    {
                        // Nếu chị KHÔNG dùng DataSource:
                        lboxRooms.Items.Add(roomName);

                        // Nếu chị có đặt DataSource (BindingList<string>), dùng:
                        // roomsBindingList.Add(roomName);
                    }

                }

            }
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
           MessageBox.Show("Log Out functionality is not implemented yet.",
                "Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );

        }

        private void lboxRooms_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void rtbMessList_TextChanged(object sender, EventArgs e)
        {

        }
        private void AppendChatMessage(string time, string sender, string message, bool isSelf)
        {
            rtbMessList.SelectionStart = rtbMessList.TextLength;

            // Phần thời gian
            rtbMessList.SelectionColor = Color.Gray;
            rtbMessList.AppendText($"{time} ");

            // Phần tên
            rtbMessList.SelectionColor = isSelf ? Color.Blue : Color.Purple; // xanh cho mình, tím cho người khác
            rtbMessList.AppendText($"{sender}: ");

            // Phần nội dung
            rtbMessList.SelectionColor = Color.Black;
            rtbMessList.AppendText($"{message}{Environment.NewLine}");

            // 👉 Reset màu về đen cho chắc chắn dòng sau không bị lem
            rtbMessList.SelectionColor = Color.Black;

            // Cuộn xuống cuối
            rtbMessList.SelectionStart = rtbMessList.TextLength;
            rtbMessList.ScrollToCaret();
        }

    }
}
