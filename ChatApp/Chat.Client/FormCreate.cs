using System;
using System.Windows.Forms;

namespace Chat.Client
{
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
                    MessageBox.Show("Nhập tên phòng.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
                if (txtRoomName.Text.Length > 25)
                {
                    MessageBox.Show("Tên quá dài.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
            };
        }

        // Hàm phụ trợ
        private void FrmCreate_Load(object sender, EventArgs e) { }
        private void btnOk_Click(object sender, EventArgs e) { }
        private void lblRoom_Click(object sender, EventArgs e) { }
        private void txtRoomName_TextChanged(object sender, EventArgs e) { }
        private void btnCancel_Click(object sender, EventArgs e) { }
        private void lblText_Click(object sender, EventArgs e) { }
    }
}