using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Collections.Generic;
using System.Linq;
using SDTP_Project1.Controllers;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;


namespace AQISystemUnit.Tests.Controllers
{
    public class HomeControllerTests
    {
        /// <summary>
        /// Creates a new in-memory DbContext for testing.
        /// </summary>
        private AirQualityDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AirQualityDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AirQualityDbContext(options);
        }

        [Fact]
        public async Task Index_ShouldReturnViewResult_WithGroupedSensorData()
        {
            // Arrange: seed in-memory database
            var context = GetInMemoryContext();
            var sensor1 = new Sensor
            {
                SensorID = "S1",
                City = "Colombo",
                Latitude = 6.9,
                Longitude = 79.8,
                IsActive = true,
                Description = string.Empty // satisfy required non-null property
            };
            var sensor2 = new Sensor
            {
                SensorID = "S2",
                City = "Galle",
                Latitude = 6.0,
                Longitude = 80.2,
                IsActive = false,
                Description = string.Empty
            };
            context.Sensors.AddRange(sensor1, sensor2);

            var readings = new List<AirQualityData>
            {
                new AirQualityData { MeasurementID = 1, SensorID = "S1", Timestamp = DateTime.UtcNow.AddMinutes(-1), AQI = 50, Sensor = sensor1 },
                new AirQualityData { MeasurementID = 2, SensorID = "S1", Timestamp = DateTime.UtcNow, AQI = 55, Sensor = sensor1 },
                new AirQualityData { MeasurementID = 3, SensorID = "S2", Timestamp = DateTime.UtcNow, AQI = 60, Sensor = sensor2 }
            };
            context.AirQualityData.AddRange(readings);
            await context.SaveChangesAsync();

            var controller = new HomeController(context);

            // Act
            var result = await controller.Index();

            // Assert: should be ViewResult with model
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData.Model.Should().BeAssignableTo<List<SensorDataViewModel>>();

            var model = viewResult.ViewData.Model as List<SensorDataViewModel>;
            model.Should().HaveCount(1, "only active sensors should be included");
            var sensorData = model.First();
            sensorData.SensorID.Should().Be("S1");
            sensorData.City.Should().Be("Colombo");
            sensorData.Readings.Should().HaveCount(2).And.BeInDescendingOrder(r => r.Timestamp);
        }

        [Fact]
        public void TestTailwind_ShouldReturnViewResult()
        {
            // Arrange: mock context just to satisfy constructor
            var mockContext = new Mock<AirQualityDbContext>(new DbContextOptions<AirQualityDbContext>());
            var controller = new HomeController(mockContext.Object);

            // Act
            var result = controller.TestTailwind();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
    }
}

