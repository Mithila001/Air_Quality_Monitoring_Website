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
        private readonly AirQualityDbContext _db;
        private readonly IOptionsMonitor<DevModeOptions> _settings; // ← updated

        public DevController(
            AirQualityDbContext db,
            IOptionsMonitor<DevModeOptions> settings)         // ← updated
        {
            _db = db;
            _settings = settings;
        }

        [HttpPost("toggle")]
        public IActionResult ToggleDevMode()
        {
            // Flip the in‑memory flag
            var cfg = _settings.CurrentValue;
            cfg.Enabled = !cfg.Enabled;

            return Json(new { devMode = cfg.Enabled });
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
