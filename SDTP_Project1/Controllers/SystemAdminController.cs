using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using Microsoft.AspNetCore.Identity;
using System.IO;


namespace SDTP_Project1.Controllers
{

    //[Authorize(Roles = "System Admin")]
    public class SystemAdminController : Controller
    {
        private readonly ISystemAdminRepository _systemAdminRepository; 
        private readonly ISensorRepository _sensorRepository;
        private readonly IPasswordHasher<AdminUser> _hasher;

        public SystemAdminController(ISystemAdminRepository systemAdminRepository,
            ISensorRepository sensorRepository,
            IPasswordHasher<AdminUser> hasher)
        {
            _systemAdminRepository = systemAdminRepository; 
            _sensorRepository = sensorRepository;
            _hasher = hasher;
        }

        public async Task<IActionResult> Index()
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

        [HttpPost]
        public async Task<IActionResult> EditAdmin(
        AdminUser updatedUser,
        string? NewPassword)    // ← binds to <input name="NewPassword">
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            var existingUser = await _systemAdminRepository.GetByIdAsync(updatedUser.Id);
            if (existingUser == null)
                return NotFound();

            // Update all relevant fields
            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.IsActive = updatedUser.IsActive;
            existingUser.Gender = updatedUser.Gender;
            existingUser.Age = updatedUser.Age;
            existingUser.UserRole = updatedUser.UserRole;

            // —— If a new password was entered, hash & update
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                existingUser.PasswordHash = _hasher.HashPassword(existingUser, NewPassword);
                TempData["PasswordChanged"] = "Password updated successfully.";
            }

            await _systemAdminRepository.UpdateAsync(existingUser);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            await _systemAdminRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(AdminUser adminUser)
        {
            // override the automatic validation on PasswordHash
            ModelState.Remove(nameof(AdminUser.PasswordHash));

            if (!ModelState.IsValid)
            {
                return PartialView("_addNewAdmin", adminUser); // Re-render modal with validation
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


            return RedirectToAction("Index");
        }



    }


}
