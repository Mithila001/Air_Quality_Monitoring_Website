using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using System;
using System.Linq;

namespace SDTP_Project1.Controllers
{
    public class AuthController : Controller
    {
        private readonly ISystemAdminRepository _userRepo;
        private readonly IPasswordHasher<AdminUser> _passwordHasher;

        public AuthController(
            ISystemAdminRepository userRepo,
            IPasswordHasher<AdminUser> passwordHasher)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            ViewData["ReturnUrl"] = returnUrl;

            try
            {
                // 1) Look up user by email
                var allUsers = await _userRepo.GetAllAsync();
                var user = allUsers.FirstOrDefault(u => u.Email == email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid credentials.");
                    return View();
                }

                // 2) Verify password
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                if (result != PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError("", "Invalid credentials.");
                    return View();
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "This account is deactivated. Please contact an administrator.");
                    return View();
                }

                // 3) Build claims & sign in
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.UserRole)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                // 4) Redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // choose based on role
                var dest = user.UserRole == "System Admin" ? "SystemAdmin" : "Admin";
                return RedirectToAction("Index", dest);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}