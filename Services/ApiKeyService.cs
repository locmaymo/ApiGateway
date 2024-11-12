using ApiGateway.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NuGet.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiGateway.Services
{
    public class ApiKeyService
    {
        private readonly IMongoCollection<User> _users;

        public ApiKeyService(IMongoClient client, IOptions<MongoDbSettings> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public async Task<string> GenerateApiKeyAsync(string userId)
        {
            // Generate a new API key
            var apiKey = GenerateSecureApiKey();

            // Hash the API key before storing
            var apiKeyHash = HashApiKey(apiKey);

            // Update the user's API key in the database
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.ApiKey, apiKeyHash);

            await _users.UpdateOneAsync(filter, update);

            return apiKey;
        }

        public async Task<User> GetUserByApiKeyAsync(string apiKey)
        {
            var apiKeyHash = HashApiKey(apiKey);

            return await _users.Find(u => u.ApiKey == apiKeyHash).FirstOrDefaultAsync();
        }

        private string GenerateSecureApiKey()
        {
            const string prefix = "escs-";
            const int keyLength = 32; // Length of the random part of the API key

            // Generate secure random bytes
            byte[] randomBytes = new byte[keyLength];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Convert bytes to a base64 string to get a printable representation
            string randomPart = Convert.ToBase64String(randomBytes);

            // Remove any characters that are not URL safe (optional)
            randomPart = RemoveNonUrlSafeCharacters(randomPart);

            // Combine the prefix and the random part
            string apiKey = prefix + randomPart;

            return apiKey;
        }

        private string RemoveNonUrlSafeCharacters(string input)
        {
            // Replace '+' with '-', '/' with '_', and remove '='
            return input.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        // Hash the API key before storing
        private string HashApiKey(string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}