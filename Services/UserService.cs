using ApiGateway.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Threading.Tasks;


namespace ApiGateway.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoClient client, IOptions<MongoDbSettings> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public async Task CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        // GetUserByIdAsync
        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        // GetUserListAsync
        public async Task<List<User>> GetUserListAsync()
        {
            return await _users.Find(u => true).ToListAsync();
        }

        public Task UpdateUserAsync(User user) {
            // Update user in database
            return _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        // You can add more methods like UpdateUserAsync, DeleteUserAsync, etc.
    }
}