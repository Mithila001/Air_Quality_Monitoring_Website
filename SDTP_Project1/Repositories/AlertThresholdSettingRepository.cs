using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDTP_Project1.Repositories
{
    public class AlertThresholdSettingRepository : IAlertThresholdSettingRepository
    {
        private readonly AirQualityDbContext _context;

        public AlertThresholdSettingRepository(AirQualityDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AlertThresholdSetting entity)
        {
            _context.AlertThresholdSettings.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.AlertThresholdSettings.FindAsync(id);
            if (entity != null)
            {
                _context.AlertThresholdSettings.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AlertThresholdSetting>> GetAllAsync()
        {
            return await _context.AlertThresholdSettings.ToListAsync();
        }

        public async Task<AlertThresholdSetting> GetByParameterAsync(string parameter)
        {
            return await _context.AlertThresholdSettings.FirstOrDefaultAsync(s => s.Parameter == parameter);
        }

        public async Task UpdateAsync(AlertThresholdSetting entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}