using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class Chat_TCP_Client : Form

       
    {
        public Chat_TCP_Client() => InitializeComponent();

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Connect_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnLeave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Leave Room functionality is not implemented yet.",
                "Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );

        }

        private void txtMessageInput_TextChanged(object sender, EventArgs e)
        {


        }

        private void pnlMessageList_Paint(object sender, PaintEventArgs e)
        {
            

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessInput.Text))
                return;

            string senderName = "Linh";
            string time = DateTime.Now.ToString("HH:mm:ss");
            string message = txtMessInput.Text.Trim();

            AppendChatMessage(time, senderName, message, true); // true = tin của mình

            txtMessInput.Clear();
        }

        private void pnlChatFame_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlChatHeader_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lboxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Chat_TCP_Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to close the chat?", 
                "Confirm Leave", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                {
                e.Cancel = false; // Hủy đóng form
            }
            else if(result == DialogResult.No)    
            {
                e.Cancel = true; // Giữ form mở
            }


        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Join Room functionality is not implemented yet.",
                "Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );
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
