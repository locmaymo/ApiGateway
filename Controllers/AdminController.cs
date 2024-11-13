using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ApiGateway.Models;
using ApiGateway.Services;
using ApiGateway.Helpers;

namespace ApiGateway.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ có thể truy cập với những ai có Role là Admin
    public class AdminController : Controller
    {
        private readonly UserService _userService;

        public AdminController(UserService userService)
        {
            _userService = userService;
        }

        // GET: /Admin/CreateUser
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra xem username đã tồn tại trong hệ thống chưa
            var existingUser = await _userService.GetUserByUsernameAsync(model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            // Sinh Salt ngẫu nhiên cho mật khẩu
            var salt = PasswordHelper.GenerateSalt();

            // Hash mật khẩu với Salt mới sinh
            var hashedPassword = PasswordHelper.HashPasswordWithSalt(model.Password, salt);

            // Tạo đối tượng User mới
            var user = new User
            {
                Username = model.Username,
                Name = model.Name,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,  // Lưu Salt kèm theo mật khẩu đã hash
                Role = model.Role      // ví dụ "User" hoặc "Admin"
            };

            // Lưu người dùng vào cơ sở dữ liệu
            await _userService.CreateUserAsync(user);

            // Điều hướng hoặc hiển thị thông báo thành công
            return RedirectToAction("UserList"); // Giả sử bạn có action UserList
        }

        // GET: /Admin/UserList
        [HttpGet]
        public async Task<IActionResult> UserList()
        {
            // Thực hiện các thao tác để lấy danh sách người dùng từ cơ sở dữ liệu
            var userList = await _userService.GetUserListAsync();

            // Trả về view chứa danh sách người dùng
            return View(userList);
        }

        // GET: /Admin/EditUser/{id}
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminEditUserModel
            {
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                //Role = user.Role
            };

            // Kiểm tra nếu cấu hình SMTP đã tồn tại
            if (user.SmtpConfig != null)
            {
                model.SmtpHost = user.SmtpConfig.Host;
                model.SmtpPort = user.SmtpConfig.Port;
                model.SmtpUsername = user.SmtpConfig.Username;
                model.SmtpPassword = user.SmtpConfig.Password; // Chú ý không hiển thị mật khẩu nếu không cần thiết
                model.SmtpUseSSL = user.SmtpConfig.UseSSL;
                model.SmtpUseTLS = user.SmtpConfig.UseTLS;
            }

            return View(model);
        }

        // POST: /Admin/EditUser/{id}
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(AdminEditUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.GetUserByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user information
            user.Username = model.Username;
            user.Name = model.Name;
            user.Email = model.Email;
            //user.Role = model.Role;

            // Update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                var salt = PasswordHelper.GenerateSalt();
                user.PasswordHash = PasswordHelper.HashPasswordWithSalt(model.Password, salt);
                user.PasswordSalt = salt;
            }

            // Update SMTP Config
            if (!string.IsNullOrEmpty(model.SmtpHost) && model.SmtpPort.HasValue)
            {
                user.SmtpConfig = new SmtpConfiguration
                {
                    Host = model.SmtpHost,
                    Port = model.SmtpPort.Value,
                    Username = model.SmtpUsername,
                    Password = model.SmtpPassword, // Lưu mật khẩu - mã hóa nếu cần
                    UseSSL = model.SmtpUseSSL,
                    UseTLS = model.SmtpUseTLS
                };
            }

            await _userService.UpdateUserAsync(user);

            return RedirectToAction("UserList");
        }

        // GET: /Admin/ApiLogs
        [HttpGet]
        public IActionResult ApiLogs()
        {
            // Hiển thị danh sách logs API
            return View();
        }


        // Các action khác như UserList, EditUser, DeleteUser v.v.
    }
}