using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;

namespace SDTP_Project1.Controllers
{

    public class SystemAdminController : Controller
    {
        private readonly ISystemAdminRepository _systemAdminRepository; //Changed

        public SystemAdminController(ISystemAdminRepository systemAdminRepository) //Changed
        {
            _systemAdminRepository = systemAdminRepository; //Changed
        }

        public async Task<IActionResult> Index()
        {
            var adminUsers = await _systemAdminRepository.GetAllAsync(); //Changed
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


    }


}
