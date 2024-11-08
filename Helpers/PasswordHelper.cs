using System.Security.Cryptography;
using System.Text;

namespace ApiGateway.Helpers
{
    public static class PasswordHelper
    {
        // Hàm tạo Salt ngẫu nhiên (mã hóa base64)
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[128]; // Kích thước của salt là 128 byte
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Hàm băm mật khẩu với salt
        public static string HashPasswordWithSalt(string password, string salt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(salt)))
            {
                var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedPassword);
            }
        }

        // Kiểm tra băm mật khẩu với salt
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var computedHash = HashPasswordWithSalt(password, storedSalt);
            return storedHash == computedHash;
        }
    }
}