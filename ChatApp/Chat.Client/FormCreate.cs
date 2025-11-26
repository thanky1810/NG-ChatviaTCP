// File: Chat.Client/FormCreate.cs
// (Người 6 - Cao Xuân Quyết: Logic Form Tạo phòng)
using System;
using System.Windows.Forms;

namespace Chat.Client
{
    public partial class FormCreate : Form
    {
        // (Người 6) Các thuộc tính để lấy dữ liệu ra ngoài
        public string RoomName => txtRoomName.Text.Trim();
        public string RoomPassword => txtPassword.Text.Trim();

        public FormCreate()
        {
            InitializeComponent();
            btnOk.DialogResult = DialogResult.OK;

            // (Người 6) Xử lý sự kiện Click và Validation dữ liệu
            btnOk.Click += (s, e) =>
            {
                // Kiểm tra tên phòng
                if (string.IsNullOrWhiteSpace(txtRoomName.Text))
                {
                    MessageBox.Show("Nhập tên phòng.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
                if (txtRoomName.Text.Length > 25)
                {
                    MessageBox.Show("Tên phòng quá dài (max 25).", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                // (Người 6) Kiểm tra độ dài mật khẩu
                if (txtPassword.Text.Length > 25)
                {
                    MessageBox.Show("Mật khẩu quá dài (tối đa 25 ký tự).", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None; // Giữ form lại
                    return;
                }
            };
        }

        // (Người 6) Các hàm UI phụ trợ (để tránh lỗi Designer)
        private void FrmCreate_Load(object sender, EventArgs e) { }
        private void btnOk_Click(object sender, EventArgs e) { }
        private void lblRoom_Click(object sender, EventArgs e) { }
        private void txtRoomName_TextChanged(object sender, EventArgs e) { }
        private void btnCancel_Click(object sender, EventArgs e) { }
        private void lblText_Click(object sender, EventArgs e) { }
    }
}