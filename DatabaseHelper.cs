using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MoNuong
{
    internal class DatabaseHelper
    {
        private readonly string connectionString = "Data Source=AN-BUDO;Initial Catalog=MoNuong;Integrated Security=True";

        public DatabaseHelper() { }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // ---  SHA-256 ---
        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Chuyển mật khẩu thành mảng byte
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển mảng byte thành chuỗi Hex
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // --- HÀM THỰC THI TRUY VẤN CHUNG ---
        public DataTable ExecuteQuery(string query)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi Database: " + ex.Message, "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        public int ExecuteNonQuery(string query)
        {
            using (SqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thực thi: " + ex.Message);
                    return -1;
                }
            }
        }

        // --- QUẢN LÝ ĐĂNG NHẬP & TÀI KHOẢN ---
        public (int userId, int roleId, string hoTen, string email, string soDienThoai) CheckLogin(string input, string password)
        {
            // Mã hóa mật khẩu đầu vào để so sánh với bản băm trong DB
            string hashedPassword = HashPassword(password);

            using (SqlConnection conn = GetConnection())
            {
                string query = @"
                    SELECT UserID, RoleID, HoTen, Email, SoDienThoai
                    FROM Users
                    WHERE (Email = @Input OR SoDienThoai = @Input) AND MatKhau = @MatKhau";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Input", input);
                cmd.Parameters.AddWithValue("@MatKhau", hashedPassword);

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (
                                Convert.ToInt32(reader["UserID"]),
                                Convert.ToInt32(reader["RoleID"]),
                                reader["HoTen"].ToString(),
                                reader["Email"].ToString(),
                                reader["SoDienThoai"].ToString()
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối khi đăng nhập: " + ex.Message);
                }
                return (-1, -1, null, null, null);
            }
        }

        public void InsertAccount(string hoTen, DateTime ngaySinh, string email, string sdt, string password, int roleId)
        {
            // Mã hóa mật khẩu trước khi lưu
            string hashedPassword = HashPassword(password);

            using (SqlConnection conn = GetConnection())
            {
                string query = @"INSERT INTO Users(HoTen, NgaySinh, Email, SoDienThoai, MatKhau, RoleID) 
                                 VALUES(@HoTen, @NgaySinh, @Email, @SDT, @MatKhau, @RoleID)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HoTen", hoTen);
                cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@SDT", sdt);
                cmd.Parameters.AddWithValue("@MatKhau", hashedPassword);
                cmd.Parameters.AddWithValue("@RoleID", roleId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đăng ký tài khoản: " + ex.Message);
                }
            }
        }

        public DataTable GetAllAccounts()
        {
            string query = @"SELECT u.UserID, u.HoTen, u.NgaySinh, u.Email, u.SoDienThoai, r.RoleName
                             FROM Users u
                             INNER JOIN Roles r ON u.RoleID = r.RoleID";
            return ExecuteQuery(query);
        }

        public void UpdateAccount(int userId, string hoTen, DateTime ngaySinh, string email, string sdt, string password, int roleId)
        {
            using (SqlConnection conn = GetConnection())
            {
                // Nếu mật khẩu trống thì giữ nguyên mật khẩu cũ, nếu có thì mã hóa mật khẩu mới
                bool isChangePass = !string.IsNullOrEmpty(password);
                string query = isChangePass
                    ? @"UPDATE Users SET HoTen=@HoTen, NgaySinh=@NgaySinh, Email=@Email, SoDienThoai=@SDT, MatKhau=@MatKhau, RoleID=@RoleID WHERE UserID=@UserID"
                    : @"UPDATE Users SET HoTen=@HoTen, NgaySinh=@NgaySinh, Email=@Email, SoDienThoai=@SDT, RoleID=@RoleID WHERE UserID=@UserID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@HoTen", hoTen);
                cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@SDT", sdt);
                cmd.Parameters.AddWithValue("@RoleID", roleId);
                cmd.Parameters.AddWithValue("@UserID", userId);

                if (isChangePass)
                {
                    cmd.Parameters.AddWithValue("@MatKhau", HashPassword(password));
                }

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi cập nhật tài khoản: " + ex.Message);
                }
            }
        }

        public void DeleteAccount(int userId)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "DELETE FROM Users WHERE UserID=@UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa tài khoản: " + ex.Message);
                }
            }
        }

        public bool CheckEmailExists(string email)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                try
                {
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch { return false; }
            }
        }
    }
}