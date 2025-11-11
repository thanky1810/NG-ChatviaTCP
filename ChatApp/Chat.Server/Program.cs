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

        // Khởi động Giao diện Quản lý (Dashboard) của Server
        Application.Run(new ServerForm());
    }
}