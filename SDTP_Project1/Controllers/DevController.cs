using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SDTP_Project1.Data;
using SDTP_Project1.Models;

namespace SDTP_Project1.Controllers
{
    [Route("dev")]
    [Authorize(Roles = "System Admin")] // Fix: Added authorization to prevent unauthorized access
    public class DevController : Controller
    {
        private readonly DevModeState _state;
        private readonly AirQualityDbContext _db;

        public DevController(AirQualityDbContext db, DevModeState state)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        [HttpPost("toggle")]
        public IActionResult ToggleDevMode()
        {
            try
            {
                _state.Enabled = !_state.Enabled;
                return Json(new { success = true, devMode = _state.Enabled });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to toggle dev mode", error = ex.Message });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                return Json(new { success = true, devMode = _state.Enabled });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to get dev mode status", error = ex.Message });
            }
        }

        [HttpPost("clear-today")]
        public async Task<IActionResult> ClearTodayAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var toDelete = await _db.AirQualityData
                    .Where(a => a.Timestamp >= today && a.Timestamp < today.AddDays(1))
                    .ToListAsync();

                _db.AirQualityData.RemoveRange(toDelete);
                await _db.SaveChangesAsync();
                return Json(new { success = true, deleted = toDelete.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to clear today's data", error = ex.Message });
            }
        }
    }
}