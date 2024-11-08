namespace ApiGateway.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; } // Thời gian hết hạn token
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; } // Token có bị thu hồi hay không
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;
    }
}
