using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MoNuong
{
    public partial class ForgotPasswordForm : Form
    {
        private readonly Random _rng = new Random();
        private string _verificationCode;
        private string _userEmail;
        private bool _isVerified = false;

        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSendCode_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập email.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Regex kiểm tra định dạng Email chuẩn
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không đúng định dạng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DatabaseHelper db = new DatabaseHelper();
            if (!db.CheckEmailExists(email))
            {
                MessageBox.Show("Email này chưa được đăng ký trong hệ thống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _userEmail = email;
            _verificationCode = _rng.Next(100000, 999999).ToString();

            try
            {
                // Gửi Email xác thực
                SendVerificationEmail(email, _verificationCode);
                MessageBox.Show($"Mã xác thực đã được gửi tới email {email}.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Hiển thị panel nhập mã và khóa phần nhập email
                panelVerifyCode.Visible = true;
                txtEmail.Enabled = false;
                btnSendCode.Enabled = false;
            }
            catch (Exception ex)
            {
                // Trong trường hợp lỗi mạng/SMTP, hiển thị mã trực tiếp để debug (chỉ dùng khi phát triển)
                MessageBox.Show($"Lỗi gửi mail: {ex.Message}\n\nMã dự phòng: {_verificationCode}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                panelVerifyCode.Visible = true;
                txtEmail.Enabled = false;
                btnSendCode.Enabled = false;
            }
        }

        private void btnVerifyCode_Click(object sender, EventArgs e)
        {
            if (txtCode.Text.Trim() == _verificationCode)
            {
                _isVerified = true;
                MessageBox.Show("Xác thực thành công! Hãy đặt mật khẩu mới.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                panelNewPassword.Visible = true;
                panelVerifyCode.Enabled = false;
            }
            else
            {
                MessageBox.Show("Mã xác thực không chính xác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (!_isVerified) return;

            string newPass = txtNewPassword.Text.Trim();
            string confirmPass = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPass) || newPass.Length < 6)
            {
                MessageBox.Show("Mật khẩu mới phải từ 6 ký tự trở lên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Xác nhận mật khẩu không khớp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DatabaseHelper db = new DatabaseHelper();
                // Lấy thông tin User hiện tại từ Database thông qua Email
                var userTable = db.GetUserByEmail(_userEmail);

                if (userTable == null || userTable.Rows.Count == 0)
                {
                    MessageBox.Show("Lỗi: Không tìm thấy dữ liệu người dùng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Lấy dòng đầu tiên từ DataTable
                var row = userTable.Rows[0];

                // Cập nhật lại mật khẩu mới vào DB
                db.UpdateAccount(
                    Convert.ToInt32(row["UserID"]),
                    row["HoTen"].ToString(),
                    row["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(row["NgaySinh"]) : DateTime.Now,
                    row["Email"].ToString(),
                    row["SoDienThoai"].ToString(),
                    newPass, // Mật khẩu mới
                    Convert.ToInt32(row["RoleID"])
                );

                MessageBox.Show("Đã khôi phục mật khẩu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật mật khẩu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendVerificationEmail(string toEmail, string code)
        {
            // Thông tin gửi mail của bạn
            string fromEmail = "buianhx0004@gmail.com";
            string appPassword = "cuse lgjl zrij xizk"; // App Password đã tạo

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail, "Movie Ticket App");
                mail.To.Add(toEmail);
                mail.Subject = "[Xác thực] Khôi phục mật khẩu Movie Ticket App";
                mail.Body = $"Chào bạn,\n\nMã xác thực để đặt lại mật khẩu của bạn là: {code}\n\nTrân trọng!";
                mail.IsBodyHtml = false;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        private void ForgotPasswordForm_Load(object sender, EventArgs e)
        {
            panelVerifyCode.Visible = false;
            panelNewPassword.Visible = false;
        }
    }
}