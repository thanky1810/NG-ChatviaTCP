// File: Chat.Server/Program.cs
// (Người 1 - Vũ Trí Dũng: Entry Point)
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

        // (Người 1: Chạy Server Form)
        Application.Run(new ServerForm());
    }
}