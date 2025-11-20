// File: UI.Chat/FormJoinPassword.cs
using System;
using System.Windows.Forms;

namespace ClientChat // (Hoặc Chat.Client)
{
    public partial class FormJoinPassword : Form
    {
        // Property để lấy mật khẩu ra ngoài
        public string Password => txtPass.Text.Trim();

        public FormJoinPassword(string roomName)
        {
            InitializeComponent();
            this.Text = $"Vào phòng: {roomName}";
        }
    }
}