using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SDTP_Project1.Controllers
{
    [Authorize(Roles = "System Admin")]
    public class SystemAdminController : Controller
    {
        private readonly ISystemAdminRepository _systemAdminRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly IPasswordHasher<AdminUser> _hasher;

        public SystemAdminController(
            ISystemAdminRepository systemAdminRepository,
            ISensorRepository sensorRepository,
            IPasswordHasher<AdminUser> hasher)
        {
            _systemAdminRepository = systemAdminRepository ?? throw new ArgumentNullException(nameof(systemAdminRepository));
            _sensorRepository = sensorRepository ?? throw new ArgumentNullException(nameof(sensorRepository));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Get your existing list
                var adminUsers = await _systemAdminRepository.GetAllAsync();

                // 2. Get sensors
                var sensors = await _sensorRepository.GetAllSensorsAsync();

                // 3. Compute counts into ViewBag
                ViewBag.TotalUserAccounts = adminUsers.Count();
                ViewBag.TotalUserAdmins = adminUsers.Count(u => u.UserRole == "User Admin");
                ViewBag.TotalSystemAdmins = adminUsers.Count(u => u.UserRole == "System Admin");
                ViewBag.TotalActiveAdmins = adminUsers.Count(u => u.IsActive);
                ViewBag.TotalDeactiveAdmins = adminUsers.Count(u => !u.IsActive);
                ViewBag.TotalActiveSensors = sensors.Count(s => s.IsActive);
                ViewBag.TotalDeactivatedSensors = sensors.Count(s => !s.IsActive);

                // 4. Return exactly what you did before
                return View(adminUsers);
            }
            catch (Exception ex)
            {
                // Log the error
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmin(
            int id,
            string? NewPassword)    // ← binds to <input name="NewPassword">
        {
            // 1) Fetch the existing user
            var existingUser = await _systemAdminRepository.GetByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            // 2) Bind only these properties via expression lambdas:
            if (!await TryUpdateModelAsync(
                existingUser,
                prefix: string.Empty,
                u => u.Name,
                u => u.Email,
                u => u.PhoneNumber,
                u => u.IsActive,
                u => u.Gender,
                u => u.Age,
                u => u.UserRole))
            {
                // Validation failed on one of those properties
                TempData["ErrorMessage"] = "Please correct the validation errors.";
                return RedirectToAction("Index");
            }

            // 3) Handle password separately
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                existingUser.PasswordHash =
                    _hasher.HashPassword(existingUser, NewPassword);
                TempData["PasswordChanged"] = "Password updated successfully.";
            }

            // 4) Save changes
            try
            {
                await _systemAdminRepository.UpdateAsync(existingUser);
                TempData["SuccessMessage"] = "Admin updated successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating admin: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid admin ID");
            }

            try
            {
                var admin = await _systemAdminRepository.GetByIdAsync(id);
                if (admin == null)
                {
                    return NotFound(); // This needs to return NotFound to match test expectations
                }

                // Prevent deleting self
                var currentUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != null && currentUserId == id.ToString())
                {
                    TempData["ErrorMessage"] = "You cannot delete your own account";
                    return RedirectToAction("Index");
                }

                await _systemAdminRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Admin deleted successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = "Error deleting admin: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(AdminUser adminUser)
        {
            if (adminUser == null)
            {
                return BadRequest("Invalid admin data");
            }

            // override the automatic validation on PasswordHash
            ModelState.Remove(nameof(AdminUser.PasswordHash));

            if (!ModelState.IsValid)
            {
                return PartialView("_addNewAdmin", adminUser); // Re-render modal with validation
            }

            try
            {
                // Check if email already exists
                var existingUsers = await _systemAdminRepository.GetAllAsync();
                if (existingUsers.Any(u => u.Email == adminUser.Email))
                {
                    ModelState.AddModelError("Email", "This email is already in use");
                    return PartialView("_addNewAdmin", adminUser);
                }

                adminUser.RegisterDate = DateTime.Now;
                adminUser.IsActive = true;

                // —— Generate one-time password
                var randomTwo = Path.GetRandomFileName().Replace(".", "").Substring(0, 2);
                var dayString = DateTime.Now.Day.ToString("D2");   // e.g. "05"
                var plainPwd = $"{adminUser.Name}{randomTwo}{dayString}";

                // —— Hash & store
                adminUser.PasswordHash = _hasher.HashPassword(adminUser, plainPwd);

                await _systemAdminRepository.AddAsync(adminUser);

                // —— Expose plain text just once
                TempData["NewAdminPassword"] = plainPwd;
                TempData["SuccessMessage"] = "Admin added successfully";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", "Error adding admin: " + ex.Message);
                return PartialView("_addNewAdmin", adminUser);
            }
        }
                public virtual new Task<bool> TryUpdateModelAsync<TModel>(
                    TModel model,
                    string prefix,
                    params System.Linq.Expressions.Expression<Func<TModel, object>>[] includeExpressions)
                    where TModel : class
                {
                    return base.TryUpdateModelAsync(model, prefix, includeExpressions);
                }
    }
}