using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;

namespace SDTP_Project1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AirQualityDbContext _context;

        // DI for the DB context
        public HomeController(AirQualityDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            // Retrieve 5 locations from the database.
            var locations = await _context.Locations.Take(5).ToListAsync();
            return View(locations);
        }
    }
}
