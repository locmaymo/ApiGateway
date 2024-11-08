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

        public List<string> RefreshTokens { get; set; } = new List<string>();
    }
}