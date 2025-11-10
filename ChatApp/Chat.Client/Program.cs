// File: UI.Chat/Program.cs
using Chat.Client;
using Chat.Shared; // ✅ Thêm
using System;
using System.Windows.Forms;

namespace Chat.Client
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ✅ SỬA LỖI LOGOUT: Thêm vòng lặp
            while (true)
            {
                using (var login = new FormLogin())
                {
                    // Hiển thị modal; chỉ tiếp tục khi người dùng nhấn CONNECT
                    if (login.ShowDialog() != DialogResult.OK)
                        return; // Nếu người dùng đóng FormLogin, thoát hẳn

                    // ✅ SỬA LỖI DANH SÁCH USER: Truyền LoginOkDetails
                    var chat = new Chat_TCP_Client
                    {
                        UserName = login.UserName,
                        Client = login.ConnectedClient,
                        InitialLoginOk = login.LoginOkDetails // Lấy danh sách ban đầu
                    };

                    Application.Run(chat); // Chạy FormChat

                    // Sau khi FormChat (chat) bị đóng (do Logout),
                    // vòng lặp 'while' sẽ chạy lại và hiển thị FormLogin mới
                }
            }
        }
    }
}