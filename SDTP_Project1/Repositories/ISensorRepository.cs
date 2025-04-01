using System.Collections.Generic;
using System.Threading.Tasks;
using SDTP_Project1.Models;

namespace SDTP_Project1.Repositories;

public interface ISensorRepository
{
    Task<IEnumerable<Sensor>> GetAllSensorsAsync();
    Task<Sensor> GetSensorByIdAsync(string sensorId);
    Task AddSensorAsync(Sensor sensor);
    Task UpdateSensorAsync(Sensor sensor);
    Task DeactivateSensorAsync(string sensorId);
    Task DeleteSensorAsync(string sensorId);
}
