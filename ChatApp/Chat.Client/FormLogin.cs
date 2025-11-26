// File: Chat.Client/FormLogin.cs
// (Người 6 - Cao Xuân Quyết: Logic Màn hình Login & Tích hợp UC-01)
using Chat.Shared;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat.Client; // Namespace chuẩn cho project này

public partial class FormLogin : Form
{
    // (Người 6: Tích hợp Lõi Client)
    private ChatClient _chatClient;
    private CancellationTokenSource? _cts;
    private TaskCompletionSource<BaseMessage>? _loginResponseTcs;

    // (Người 6: Các thuộc tính công khai để Program.cs truy cập)
    public ChatClient ConnectedClient => _chatClient;
    public LoginOkMessage? LoginOkDetails { get; private set; }
    public string Host => txtHost.Text.Trim();
    public int Port => int.TryParse(txtPort.Text.Trim(), out int p) ? p : 0;
    public string UserName => txtUserName.Text.Trim();

    public FormLogin()
    {
        InitializeComponent();

        // (Người 6: Cài đặt giá trị mặc định để test)
        this.txtHost.Text = "127.0.0.1";
        this.txtPort.Text = "8888";
        this.txtUserName.Text = "User_Client2";

        // (Người 6: Khởi tạo Client và đăng ký sự kiện)
        _chatClient = new ChatClient();
        this.btnConnect.Click += BtnConnect_Click;

        _chatClient.ConnectionStatusChanged += OnConnectionStatusChanged;
        _chatClient.MessageReceived += OnMessageReceived;

        // (Người 6: Cài đặt giao diện)
        this.btnConnect.TabStop = false;
        this.txtHost.TabIndex = 0;
        this.txtPort.TabIndex = 1;
        this.txtUserName.TabIndex = 2;
        this.Shown += FormLogin_Shown;
        this.txtHost.KeyDown += TxtHost_KeyDown;
        this.txtPort.KeyDown += TxtPort_KeyDown;
        this.txtUserName.KeyDown += TxtUserName_KeyDown;
        this.txtHost.MaxLength = 15;
        this.txtUserName.MaxLength = 15;
        this.txtPort.MaxLength = 4;
        this.txtPort.KeyPress += TxtPort_KeyPress;
        this.txtPort.TextChanged += TxtPort_TextChanged;
    }

    // (Người 6: Xử lý cập nhật trạng thái an toàn, tránh lỗi khi form đóng)
    private void OnConnectionStatusChanged(string status)
    {
        if (!this.IsDisposed && this.IsHandleCreated)
        {
            this.BeginInvoke((Action)(() => lblHost1.Text = status));
        }
    }

    // (Người 6: Xử lý sự kiện nút Kết nối - UC-01)
    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtHost.Text) ||
            string.IsNullOrWhiteSpace(txtPort.Text) ||
            string.IsNullOrWhiteSpace(txtUserName.Text))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Cảnh báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnConnect.Enabled = false;
        _cts = new CancellationTokenSource();
        _loginResponseTcs = new TaskCompletionSource<BaseMessage>();

        try
        {
            // (Người 6 gọi hàm Connect của Người 5)
            await _chatClient.ConnectAsync(Host, Port, UserName);

            // (Người 6: Chờ phản hồi từ Server trong 5s)
            if (await Task.WhenAny(_loginResponseTcs.Task, Task.Delay(5000, _cts.Token)) != _loginResponseTcs.Task)
            {
                throw new TimeoutException("Server không phản hồi.");
            }

            // (Người 6: Xử lý kết quả đăng nhập)
            var response = await _loginResponseTcs.Task;
            if (response is LoginOkMessage)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (response is ErrorMessage err)
            {
                throw new Exception(err.Message);
            }
            else
            {
                throw new Exception("Phản hồi không hợp lệ.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Đăng nhập thất bại: {ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _chatClient.Disconnect();
            _cts?.Cancel();
        }
        finally
        {
            btnConnect.Enabled = true;
        }
    }

    // (Người 6: Nhận tin nhắn phản hồi Login)
    private void OnMessageReceived(BaseMessage message)
    {
        if (message is LoginOkMessage lok)
        {
            this.LoginOkDetails = lok;
            _loginResponseTcs?.TrySetResult(lok);
        }
        else if (message is ErrorMessage err)
        {
            _loginResponseTcs?.TrySetResult(err);
        }
    }

    // (Người 6: Dọn dẹp sự kiện khi đóng form)
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (_chatClient != null)
        {
            _chatClient.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _chatClient.MessageReceived -= OnMessageReceived;
        }
        _cts?.Cancel();
        base.OnFormClosed(e);
    }

    #region (Người 6: Các hàm xử lý sự kiện UI phụ trợ)
    private void FormLogin_Load(object sender, EventArgs e) { }

    private void FormLogin_Shown(object sender, EventArgs e)
    {
        this.BeginInvoke((Action)(() => { this.ActiveControl = txtHost; txtHost.Focus(); txtHost.SelectAll(); }));
    }

    private void TxtHost_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down) { e.SuppressKeyPress = true; txtPort.Focus(); txtPort.SelectAll(); }
        else if (e.KeyCode == Keys.Up) { e.SuppressKeyPress = true; }
    }

    private void TxtPort_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down) { e.SuppressKeyPress = true; txtUserName.Focus(); txtUserName.SelectAll(); }
        else if (e.KeyCode == Keys.Up) { e.SuppressKeyPress = true; txtHost.Focus(); txtHost.SelectAll(); }
    }

    private void TxtUserName_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up) { e.SuppressKeyPress = true; txtPort.Focus(); txtPort.SelectAll(); }
        else if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnConnect_Click(this.btnConnect, EventArgs.Empty); }
    }

    private void TxtPort_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) { e.Handled = true; }
    }

    private void TxtPort_TextChanged(object sender, EventArgs e)
    {
        var txt = txtPort.Text;
        if (string.IsNullOrEmpty(txt)) return;
        var digitsOnlyChars = new System.Text.StringBuilder(txt.Length);
        foreach (char c in txt) { if (char.IsDigit(c)) digitsOnlyChars.Append(c); }
        var digitsOnly = digitsOnlyChars.ToString();
        if (digitsOnly.Length > 4) digitsOnly = digitsOnly.Substring(0, 4);
        if (digitsOnly != txt) { int selStart = txtPort.SelectionStart; txtPort.Text = digitsOnly; txtPort.SelectionStart = Math.Min(selStart, txtPort.Text.Length); }
    }

    private void textBox1_TextChanged(object sender, EventArgs e) { }
    private void label2_Click(object sender, EventArgs e) { }
    private void panelLogin_Paint(object sender, PaintEventArgs e) { }
    private void txtHost_TextChanged(object sender, EventArgs e) { }
    private void lblLogin_Click(object sender, EventArgs e) { }
    #endregion
}