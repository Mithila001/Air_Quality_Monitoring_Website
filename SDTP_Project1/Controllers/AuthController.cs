using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;             // ← for IPasswordHasher, PasswordVerificationResult
using System.Security.Claims;
using System.Threading.Tasks;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;

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
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            string email,
            string password,
            string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
