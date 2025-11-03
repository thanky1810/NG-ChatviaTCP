using System;
using System.Windows.Forms;

namespace ClientChat
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var login = new FormLogin())
            {
                // Hiển thị modal; chỉ tiếp tục khi người dùng nhấn CONNECT (DialogResult.OK)
                if (login.ShowDialog() != DialogResult.OK)
                    return;

                var chat = new Chat_TCP_Client
                {
                    UserName = login.UserName
                    // nếu cần: Host = login.Host, Port = login.Port
                };

                Application.Run(chat);
            }
        }
    }
}
