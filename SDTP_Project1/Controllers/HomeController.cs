using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using SDTP_Project1.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SDTP_Project1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AirQualityDbContext _context;
        private readonly ISensorService _sensorService;

        public HomeController(AirQualityDbContext context, ISensorService _sensorService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            this._sensorService = _sensorService ?? throw new ArgumentNullException(nameof(_sensorService));

        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _context.AirQualityData
                    .Include(a => a.Sensor)
                    .Where(a => a.Sensor.IsActive) // Filter for active sensors at database level
                    .ToListAsync();

                // Group by SensorID and create a view model for each sensor.
                var sensorData = data
                    .GroupBy(a => a.SensorID)
                    .Select(g => new SensorDataViewModel
                    {
                        SensorID = g.Key,
                        City = g.First().Sensor.City,
                        Latitude = g.First().Sensor.Latitude,
                        Longitude = g.First().Sensor.Longitude,
                        Readings = g.OrderByDescending(r => r.Timestamp)
                                    .Take(30) // Take the latest 30 readings
                                    .ToList()
                    })
                    .ToList();

                return View(sensorData);
            }
            catch (Exception ex)
            {
                // Log the exception
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public IActionResult TestTailwind()
        {
            return View();
        }

        [HttpGet]
        [Route("error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }


        // This action is used to fetch the latest readings for a specific sensor.
        [HttpGet]
        public async Task<IActionResult> SensorDetails(string sensorId)
        {
            if (string.IsNullOrWhiteSpace(sensorId))
                return BadRequest("Sensor ID is required.");

            var readings = await _sensorService.GetLatestReadingsAsync(sensorId, 500);
            return PartialView("_SensorDetailsPartial", readings);
        }
    }
}