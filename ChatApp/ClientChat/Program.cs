// File: UI.Chat/Program.cs
using Chat.Shared;
using System;
using System.Windows.Forms;

namespace ClientChat;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // (Người 6: Cao Xuân Quyết - Vòng lặp xử lý Logout quay về Login)
        while (true)
        {
            // (Người 6) Hiển thị Form Login
            using (var login = new FormLogin())
            {
                if (login.ShowDialog() != DialogResult.OK)
                    return; // Nếu tắt form login -> Thoát luôn

                // (Người 6) Nếu login OK -> Mở Form Chat
                var chat = new Chat_TCP_Client
                {
                    UserName = login.UserName,
                    Client = login.ConnectedClient,
                    InitialLoginOk = login.LoginOkDetails
                };

                Application.Run(chat);
                // (Người 6) Khi FormChat đóng, vòng lặp while sẽ quay lại mở FormLogin
            }
        }
    }
}