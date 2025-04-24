// Repositories/ISystemAdminRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using SDTP_Project1.Models;

namespace SDTP_Project1.Repositories
{
    public interface ISystemAdminRepository
    {
        Task<IEnumerable<AdminUser>> GetAllAsync();
        Task<AdminUser?> GetByIdAsync(int id);
        Task UpdateAsync(AdminUser user);
        Task DeleteAsync(int id);
    }
}