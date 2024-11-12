namespace ApiGateway.Models
{
    public class UserEditModel
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class AdminEditUserModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        // Thêm các trường cấu hình SMTP
        public string SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpUseSSL { get; set; }
        public bool SmtpUseTLS { get; set; }

    }

}
