using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ApiGateway.Helpers;
using ApiGateway.Models;

namespace ApiGateway.Services
{
    public class DatabaseInitializer
    {
        private readonly IMongoCollection<User> _users;

        public DatabaseInitializer(IMongoClient client, IOptions<MongoDbSettings> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public async Task InitializeDatabaseAsync()
        {
            // Kiểm tra xem người dùng admin đã tồn tại chưa
            var adminUser = await _users.Find(u => u.Username == "admin").FirstOrDefaultAsync();

            if (adminUser == null)
            {
                Console.WriteLine("Admin user not found, creating default admin user.");

                // Tạo và sinh Salt ngẫu nhiên cho mật khẩu
                var salt = PasswordHelper.GenerateSalt();

                // Hash mật khẩu "admin" với Salt sinh ra
                var passwordHash = PasswordHelper.HashPasswordWithSalt("admin", salt);

                // Tạo đối tượng người dùng Admin
                var admin = new User
                {
                    Username = "admin",
                    Name = "ESCS",
                    Email = "admin@yourdomain.com",
                    PasswordHash = passwordHash,  // Lưu hash của mật khẩu
                    PasswordSalt = salt,          // Lưu salt vừa sinh ra
                    Role = "Admin",
                    SmtpConfig = null
                };

                // Lưu người dùng admin vào collection
                await _users.InsertOneAsync(admin);

                Console.WriteLine("Admin user created successfully.");
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }
    }
}