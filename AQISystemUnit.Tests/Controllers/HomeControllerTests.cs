using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDTP_Project1.Controllers;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using SDTP_Project1.Services;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

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

            // Mock the ISensorService
            var mockSensorService = new Mock<ISensorService>();

            var controller = new HomeController(context, mockSensorService.Object);

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
            // Arrange: mock context and service to satisfy constructor
            var mockContext = new Mock<AirQualityDbContext>(new DbContextOptions<AirQualityDbContext>());
            var mockSensorService = new Mock<ISensorService>();
            var controller = new HomeController(mockContext.Object, mockSensorService.Object);

            // Act
            var result = controller.TestTailwind();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Error_ShouldReturnViewResult_WithErrorViewModel()
        {
            // Arrange
            var mockContext = new Mock<AirQualityDbContext>(new DbContextOptions<AirQualityDbContext>());
            var mockSensorService = new Mock<ISensorService>();
            var controller = new HomeController(mockContext.Object, mockSensorService.Object);

            // Setup HttpContext to have a TraceIdentifier
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-id";
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Error(404);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Error");
            var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
            model.RequestId.Should().Be("test-trace-id");
        }

        [Fact]
        public async Task SensorDetails_ShouldReturnPartialView_WithReadings()
        {
            // Arrange
            var mockContext = new Mock<AirQualityDbContext>(new DbContextOptions<AirQualityDbContext>());
            var mockSensorService = new Mock<ISensorService>();
            var controller = new HomeController(mockContext.Object, mockSensorService.Object);

            var sensorId = "S1";
            var fakeReadings = new List<AirQualityData>
            {
                new AirQualityData { MeasurementID = 1, SensorID = sensorId, Timestamp = DateTime.UtcNow }
            };

            mockSensorService
                .Setup(s => s.GetLatestReadingsAsync(sensorId, 500))
                .ReturnsAsync(fakeReadings);

            // Act
            var result = await controller.SensorDetails(sensorId);

            // Assert
            var partialViewResult = result.Should().BeOfType<PartialViewResult>().Subject;
            partialViewResult.ViewName.Should().Be("_SensorDetailsPartial");
            partialViewResult.Model.Should().BeSameAs(fakeReadings);
        }

        [Fact]
        public async Task SensorDetails_WithEmptySensorId_ShouldReturnBadRequest()
        {
            // Arrange
            var mockContext = new Mock<AirQualityDbContext>(new DbContextOptions<AirQualityDbContext>());
            var mockSensorService = new Mock<ISensorService>();
            var controller = new HomeController(mockContext.Object, mockSensorService.Object);

            // Act
            var result = await controller.SensorDetails(string.Empty);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}