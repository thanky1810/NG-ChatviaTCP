// File: Chat.Client/Program.cs
using Chat.Shared;
using System;
using System.Windows.Forms;

namespace Chat.Client; // <-- Namespace của Project này

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // (Người 6) Vòng lặp `while(true)` cho phép tính năng Logout
        while (true)
        {
            // (Người 6) Hiển thị FormLogin (UC-01)
            using (var login = new FormLogin())
            {
                if (login.ShowDialog() != DialogResult.OK)
                    return; // Nếu người dùng đóng FormLogin, thoát hẳn

                // (Người 4 & 6) Truyền Client đã kết nối và danh sách User
                var chat = new Chat_TCP_Client
                {
                    UserName = login.UserName,
                    Client = login.ConnectedClient,
                    InitialLoginOk = login.LoginOkDetails
                };

                Application.Run(chat); // Chạy FormChat
            }
        }
    }
}