using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiGateway.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;

        public string Username { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string PasswordHash { get; set; } = default!;

        public string PasswordSalt { get; set; } = default!;

        public string Role { get; set; } = default!;

        public string ApiKey { get; set; } = default!;

        // Cấu hình SMTP (ban đầu có thể null/ trống)
        public SmtpConfiguration? SmtpConfig { get; set; } = default!;
    }

    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSSL { get; set; }
        public bool UseTLS { get; set; }
    }
}