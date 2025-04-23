using SDTP_Project1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDTP_Project1.Repositories
{
    public interface IAlertThresholdSettingRepository
    {
        Task<IEnumerable<AlertThresholdSetting>> GetAllAsync();
        Task<AlertThresholdSetting> GetByParameterAsync(string parameter);
        Task AddAsync(AlertThresholdSetting entity);
        Task UpdateAsync(AlertThresholdSetting entity);
        Task DeleteAsync(int id);
    }
}