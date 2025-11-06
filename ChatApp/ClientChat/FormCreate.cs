using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientChat
{
    public partial class FormCreate : Form
    {
        public string RoomName => txtRoomName.Text.Trim();

        public FormCreate()
        {
            InitializeComponent();
            btnOk.DialogResult = DialogResult.OK;  
            btnOk.Click += (s, e) =>
            {
                // Kiểm tra rỗng
                if (string.IsNullOrWhiteSpace(txtRoomName.Text))
                {
                    MessageBox.Show("Please enter room name.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                // ✅ Kiểm tra độ dài tối đa
                if (txtRoomName.Text.Length > 25)
                {
                    MessageBox.Show("Room name too long (max 25 chars).", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None; // giữ form lại
                    return;
                }
            };
        }

        private void FrmCreate_Load(object sender, EventArgs e)
        {

        }

        private void btnOk_Click(object sender, EventArgs e)
        {

        }

        private void lblRoom_Click(object sender, EventArgs e)
        {

        }

        private void txtRoomName_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void lblText_Click(object sender, EventArgs e)
        {

        }
    }
}
