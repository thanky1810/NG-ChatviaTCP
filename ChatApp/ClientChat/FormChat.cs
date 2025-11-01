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

            // Tạo label hiển thị 
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = $" {time} - {senderName}: {message} ";


            // Vị trí hiển thị (xếp dọc)
            lbl.Location = new Point(10, pnlMessList.Controls.Count * 25);


            // Thêm vào panel
            pnlMessList.Controls.Add(lbl);

            // Cuộn xuống cuối
            pnlMessList.ScrollControlIntoView(lbl);

            // Xóa ô nhập
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
                "Are you sure you want to leave the chat?", 
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
            MessageBox.Show("Create Room functionality is not implemented yet.");
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Log Out functionality is not implemented yet.");
        }
    }
}
