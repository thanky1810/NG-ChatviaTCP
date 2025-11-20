// File: UI.Chat/FormCreate.cs
using System;
using System.Windows.Forms;

namespace ClientChat;

public partial class FormCreate : Form
{
    public string RoomName => txtRoomName.Text.Trim();
    public string RoomPassword => txtPassword.Text.Trim();

    public FormCreate()
    {
        InitializeComponent();
        btnOk.DialogResult = DialogResult.OK;

        btnOk.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtRoomName.Text))
            {
                MessageBox.Show("Please enter room name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            if (txtRoomName.Text.Length > 25)
            {
                MessageBox.Show("Room name too long.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
        };
    }
}