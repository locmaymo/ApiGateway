﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using ApiGateway.Services;
using ApiGateway.Models;
using ApiGateway.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace ApiGateway.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Retrieve user from database
            var user = await _userService.GetUserByUsernameAsync(model.Username);

            // Kiểm tra xem người dùng có tồn tại không
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username.");
                return View(model);
            }

            // So sánh password nhập vào với hash và salt đã lưu
            if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError("", "Invalid password.");
                return View(model);
            }

            // Create claims including roles
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                // Add other claims if needed
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Keep the user logged in
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/EditProfile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var username = User.Identity.Name;
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditModel
            {
                Email = user.Email
            };

            return View(model);
        }

        // POST: /Account/EditProfile
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(UserEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Retrieve user from database
            var username = User.Identity.Name;
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            // Update email
            user.Email = model.Email;

            // Update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                var salt = PasswordHelper.GenerateSalt();
                user.PasswordHash = PasswordHelper.HashPasswordWithSalt(model.Password, salt);
                user.PasswordSalt = salt;
            }

            // Save changes
            await _userService.UpdateUserAsync(user);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}