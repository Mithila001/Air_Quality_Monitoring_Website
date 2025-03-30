using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
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
            // Retrieve all air quality records from the database.
            var data = await _context.AirQualityData.ToListAsync();
            return View(data);
        }
    }
}
