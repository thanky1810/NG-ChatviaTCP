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

        }

        private void txtMessageInput_TextChanged(object sender, EventArgs e)
        {


        }

        private void pnlMessageList_Paint(object sender, PaintEventArgs e)
        {
            

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessageInput.Text))
                return;

            string senderName = "Linh";
            string time = DateTime.Now.ToString("HH:mm:ss");
            string message = txtMessageInput.Text.Trim();

            // Tạo label hiển thị 
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = $" {time} - {senderName}: {message} ";


            // Vị trí hiển thị (xếp dọc)
            lbl.Location = new Point(10, pnlMessageList.Controls.Count * 25);


            // Thêm vào panel
            pnlMessageList.Controls.Add(lbl);

            // Cuộn xuống cuối
            pnlMessageList.ScrollControlIntoView(lbl);

            // Xóa ô nhập
            txtMessageInput.Clear();

        }

        private void pnlChatFame_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlChatHeader_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
