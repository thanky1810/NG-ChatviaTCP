// File: Chat.Server/Program.cs
using System;
using System.Windows.Forms;

namespace Chat.Server;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // ✅ SỬA: Khởi động Form Giao diện
        Application.Run(new ServerForm());
    }
}