using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SDTP_Project1.Data;
using SDTP_Project1.Models;   // ← ensure DevModeOptions is in this namespace

namespace SDTP_Project1.Controllers
{
    [Route("dev")]
    public class DevController : Controller
    {
        private readonly DevModeState _state;
        private readonly AirQualityDbContext _db;

        public DevController(AirQualityDbContext db, DevModeState state)
        {
            _db = db;
            _state = state;
        }

        [HttpPost("toggle")]
        public IActionResult ToggleDevMode()
        {
            _state.Enabled = !_state.Enabled;
            return Json(new { devMode = _state.Enabled });
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Json(new { devMode = _state.Enabled });
        }

        [HttpPost("clear-today")]
        public async Task<IActionResult> ClearTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            var toDelete = await _db.AirQualityData
                .Where(a => a.Timestamp >= today && a.Timestamp < today.AddDays(1))
                .ToListAsync();

            _db.AirQualityData.RemoveRange(toDelete);
            await _db.SaveChangesAsync();
            return Json(new { deleted = toDelete.Count });
        }
    }
}
