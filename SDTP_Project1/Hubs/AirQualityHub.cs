// Path: SDTP_Project1/Hubs/AirQualityHub.cs
using Microsoft.AspNetCore.SignalR;
using SDTP_Project1.Models;
using System.Threading.Tasks;

namespace SDTP_Project1.Hubs
{
    public class AirQualityHub : Hub
    {
        public async Task SendUpdatedData(List<SensorDataViewModel> sensorData)
        {
            await Clients.All.SendAsync("ReceiveUpdatedData", sensorData);
        }
    }
}