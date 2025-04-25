// Repositories/SystemAdminRepository.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;
using SDTP_Project1.Models;

namespace SDTP_Project1.Repositories
{
    public class SystemAdminRepository : ISystemAdminRepository
    {
        private readonly AirQualityDbContext _context;

        public SystemAdminRepository(AirQualityDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminUser>> GetAllAsync()
        {
            return await _context.AdminUsers.ToListAsync();
        }

        public async Task<AdminUser?> GetByIdAsync(int id)
        {
            return await _context.AdminUsers.FindAsync(id);
        }

        public async Task UpdateAsync(AdminUser user)
        {
            _context.AdminUsers.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.AdminUsers.FindAsync(id);
            if (user != null)
            {
                _context.AdminUsers.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(AdminUser user)
        {
            _context.AdminUsers.Add(user);
            await _context.SaveChangesAsync();
        }

    }
}