using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;

namespace SDTP_Project1.Controllers
{

    public class SystemAdminController : Controller
    {
        private readonly ISystemAdminRepository _systemAdminRepository; //Changed
        private readonly ISensorRepository _sensorRepository;

        public SystemAdminController(ISystemAdminRepository systemAdminRepository, ISensorRepository sensorRepository) //Changed
        {
            _systemAdminRepository = systemAdminRepository; //Changed
            _sensorRepository = sensorRepository;
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
        public async Task<IActionResult> EditAdmin(AdminUser updatedUser)
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
            if (!ModelState.IsValid)
            {
                return PartialView("_addNewAdmin", adminUser); // Re-render modal with validation
            }

            adminUser.RegisterDate = DateTime.Now;
            adminUser.IsActive = true;

            await _systemAdminRepository.AddAsync(adminUser);
            return RedirectToAction("Index");
        }



    }


}
