using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDTP_Project1.Data;
using SDTP_Project1.Models;

namespace SDTP_Project1.Repositories;
public class SensorRepository : ISensorRepository
{
    private readonly AirQualityDbContext _context;

    public SensorRepository(AirQualityDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sensor>> GetAllSensorsAsync()
    {
        return await _context.Sensors.ToListAsync();
    }

    public async Task<Sensor> GetSensorByIdAsync(string sensorId)
    {
        return await _context.Sensors.FindAsync(sensorId);
    }

    public async Task AddSensorAsync(Sensor sensor)
    {
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSensorAsync(Sensor sensor)
    {
        _context.Sensors.Update(sensor);
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateSensorAsync(string sensorId)
    {
        var sensor = await GetSensorByIdAsync(sensorId);
        if (sensor != null)
        {
            sensor.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}
