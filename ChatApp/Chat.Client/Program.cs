// File: Chat.Client/Program.cs
// (Người 6 - Cao Xuân Quyết: Tích hợp luồng chạy của ứng dụng)
using Chat.Client;
using Chat.Shared;
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

            // (Người 6: Cao Xuân Quyết - Vòng lặp duy trì ứng dụng để hỗ trợ tính năng Đăng xuất quay lại Đăng nhập)
            while (true)
            {
                // (Người 6: Hiển thị màn hình Đăng nhập)
                using (var login = new FormLogin())
                {
                    // (Người 6: Kiểm tra kết quả đăng nhập, nếu đóng form thì thoát chương trình)
                    if (login.ShowDialog() != DialogResult.OK)
                        return;

                    // (Người 6: Khởi tạo màn hình Chat và truyền dữ liệu kết nối từ FormLogin sang)
                    var chat = new Chat_TCP_Client
                    {
                        UserName = login.UserName,
                        Client = login.ConnectedClient,
                        InitialLoginOk = login.LoginOkDetails // (Người 6: Truyền danh sách User/Room ban đầu)
                    };

                    // (Người 6: Chạy màn hình Chat chính)
                    Application.Run(chat);

                    // (Người 6: Khi FormChat đóng (do Logout), vòng lặp sẽ quay lại mở FormLogin mới)
                }
            }
        }
    }
}