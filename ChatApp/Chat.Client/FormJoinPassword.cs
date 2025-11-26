// File: Chat.Client/FormJoinPassword.cs
// (Người 6 - Cao Xuân Quyết: Logic Form Nhập mật khẩu khi vào phòng)
using System;
using System.Windows.Forms;

namespace Chat.Client
{
    public partial class FormJoinPassword : Form
    {
        // (Người 6) Property để lấy mật khẩu ra ngoài cho FormChat sử dụng
        public string Password => txtPass.Text.Trim();

        public FormJoinPassword(string roomName)
        {
            InitializeComponent();
            // (Người 6) Hiển thị tên phòng trên tiêu đề form
            this.Text = $"Vào phòng: {roomName}";
        }
    }
}