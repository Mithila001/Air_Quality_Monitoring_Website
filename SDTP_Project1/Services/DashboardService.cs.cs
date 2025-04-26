using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using SDTP_Project1.Data;
//using SDTP_Project1.Models;

//namespace SDTP_Project1.Services
//{
//    // 1) Interface and implementation in one file is acceptable for small projects:
//    public interface IDashboardService
//    {
//        Task<DashboardStatsViewModel> GetDashboardStatsAsync();
//    }

//    public class DashboardService : IDashboardService
//    {
//        private readonly AirQualityDbContext _context;

//        public DashboardService(AirQualityDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
//        {
//            return new DashboardStatsViewModel
//            {
//                TotalUserAccounts = await _context.AdminUsers.CountAsync()
//                                           + await _context.MonitoringAdmins.CountAsync(),
//                TotalUserAdmins = await _context.MonitoringAdmins.CountAsync(),
//                TotalSystemAdmins = await _context.AdminUsers.CountAsync(),
//                TotalActiveAdmins = await _context.AdminUsers.CountAsync(u => u.IsActive),
//                TotalDeactiveAdmins = await _context.AdminUsers.CountAsync(u => !u.IsActive),
//                TotalActiveSensors = await _context.Sensors.CountAsync(s => s.IsActive),
//                TotalDeactivatedSensors = await _context.Sensors.CountAsync(s => !s.IsActive)
//            };
//        }
//    }
//}
