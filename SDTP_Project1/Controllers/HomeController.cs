using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SDTP_Project1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AirQualityDbContext _context;

        public HomeController(AirQualityDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.AirQualityData.Include(a => a.Sensor).ToListAsync();

            // Group by SensorID and create a view model for each sensor.
            var sensorData = data
                .Where(a => a.Sensor.IsActive) // Filter for active sensors
                .GroupBy(a => a.SensorID)
                .Select(g => new SensorDataViewModel
                {
                    SensorID = g.Key,
                    City = g.First().Sensor.City,
                    Latitude = g.First().Sensor.Latitude,
                    Longitude = g.First().Sensor.Longitude,
                    Readings = g.OrderByDescending(r => r.Timestamp)
                                .Take(30) // Take the latest 100 readings
                                .ToList()
                })
                .ToList();

            return View(sensorData);
        }
        public IActionResult TestTailwind()
        {
            return View();
        }
    }
}
