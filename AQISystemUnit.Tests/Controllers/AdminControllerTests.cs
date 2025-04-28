using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using SDTP_Project1.Controllers;
using SDTP_Project1.Data;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using SDTP_Project1.Services;
using Xunit;

namespace AQISystemUnit.Tests.Controllers
{
    public class AdminControllerTests : IDisposable
    {
        private readonly Mock<ISensorRepository> _mockSensorRepo;
        private readonly Mock<IAlertThresholdSettingRepository> _mockAlertRepo;
        private readonly Mock<ISensorService> _mockSensorService;
        private readonly AirQualityDbContext _dbContext;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockSensorRepo = new Mock<ISensorRepository>();
            _mockAlertRepo = new Mock<IAlertThresholdSettingRepository>();
            _mockSensorService = new Mock<ISensorService>();

            // Use InMemoryDatabase for AirQualityDbContext
            var options = new DbContextOptionsBuilder<AirQualityDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique for each test
                .Options;

            _dbContext = new AirQualityDbContext(options);

            _controller = new AdminController(
                _mockSensorRepo.Object,
                _mockAlertRepo.Object,
                _mockSensorService.Object,
                _dbContext);

            // Setup TempData
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithSensorsAndAverageAQI()
        {
            // Arrange
            var sensors = new List<Sensor>
    {
        new Sensor { SensorID = "SENSOR_001", City = "Colombo", Latitude = 6.9, Longitude = 79.8 }
    };

            _mockSensorRepo.Setup(repo => repo.GetAllSensorsAsync()).ReturnsAsync(sensors);
            _mockSensorService.Setup(service => service.GetAverageAQILast30DaysForAllSensors()).ReturnsAsync(42.0);

            _dbContext.AirQualityAlertHistory.Add(new AirQualityAlertHistory
            {
                SensorID = "SENSOR_001",
                MeasurementID = 1,
                Parameter = "PM2.5",
                CurrentValue = 120.5,
                ThresholdValue = 100,
                AlertedTime = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(sensors);

            // Correct casting here
            ((double)_controller.ViewBag.AverageAQI).Should().Be(42.0);

            ((List<AirQualityAlertHistory>)_controller.ViewBag.RecentAlerts).Should().NotBeNull();

        }


        [Fact]
        public void CreateSensor_Get_ShouldReturnView()
        {
            var result = _controller.CreateSensor();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task CreateSensor_Post_InvalidModel_ShouldReturnViewWithModel()
        {
            // Arrange
            var sensor = new Sensor { City = "" };
            _controller.ModelState.AddModelError("City", "City is required.");

            // Act
            var result = await _controller.CreateSensor(sensor);

            // Assert
            result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(sensor);
        }

        [Fact]
        public async Task CreateSensor_Post_ValidModel_ShouldSaveAndRedirect()
        {
            // Arrange
            var sensor = new Sensor { City = "Colombo", Latitude = 6.9271, Longitude = 79.8612 };
            _mockSensorRepo.Setup(repo => repo.AddSensorAsync(It.IsAny<Sensor>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateSensor(sensor);

            // Assert
            _mockSensorRepo.Verify(repo => repo.AddSensorAsync(It.IsAny<Sensor>()), Times.Once);
            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
            _controller.TempData["SuccessMessage"].Should().Be("Sensor added successfully!");
        }

        [Fact]
        public async Task EditSensor_Get_SensorExists_ShouldReturnPartialView()
        {
            var sensor = new Sensor { SensorID = "SENSOR_123" };
            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync(sensor.SensorID)).ReturnsAsync(sensor);

            var result = await _controller.EditSensor(sensor.SensorID);

            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_EditSensorPartial");
            view.Model.Should().Be(sensor);
        }

        [Fact]
        public async Task EditSensor_Get_SensorDoesNotExist_ShouldReturnNotFound()
        {
            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync("invalid_id")).ReturnsAsync((Sensor)null);

            var result = await _controller.EditSensor("invalid_id");

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task EditSensor_Post_InvalidModel_ShouldReturnPartialView()
        {
            var sensor = new Sensor { SensorID = "SENSOR_123" };
            _controller.ModelState.AddModelError("City", "City is required.");

            var result = await _controller.EditSensor(sensor);

            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_EditSensorPartial");
            view.Model.Should().Be(sensor);
        }

        [Fact]
        public async Task EditSensor_Post_ValidModel_ShouldUpdateAndReturnJson()
        {
            var sensor = new Sensor
            {
                SensorID = "SENSOR_123",
                City = "City",
                Latitude = 1.0,
                Longitude = 1.0
            };

            var existingSensor = new Sensor
            {
                SensorID = "SENSOR_123",
                City = "Old City",
                Latitude = 0.0,
                Longitude = 0.0
            };

            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync(sensor.SensorID)).ReturnsAsync(existingSensor);
            _mockSensorRepo.Setup(r => r.UpdateSensorAsync(It.IsAny<Sensor>())).Returns(Task.CompletedTask);

            var result = await _controller.EditSensor(sensor);

            _mockSensorRepo.Verify(r => r.UpdateSensorAsync(It.IsAny<Sensor>()), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var editResult = jsonResult.Value.Should().BeOfType<AdminController.EditSensorResult>().Subject;
            editResult.Success.Should().BeTrue();
            editResult.Message.Should().Be("Sensor updated successfully.");
        }

        [Fact]
        public async Task ToggleSensorStatus_SensorExists_ShouldUpdateAndReturnJson()
        {
            var sensor = new Sensor { SensorID = "SENSOR_123", IsActive = false };
            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync(sensor.SensorID)).ReturnsAsync(sensor);
            _mockSensorRepo.Setup(r => r.UpdateSensorAsync(It.IsAny<Sensor>())).Returns(Task.CompletedTask);

            var result = await _controller.ToggleSensorStatus(sensor.SensorID, true);

            _mockSensorRepo.Verify(r => r.UpdateSensorAsync(It.IsAny<Sensor>()), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var statusResult = jsonResult.Value.Should().BeOfType<AdminController.SensorStatusResult>().Subject;
            statusResult.Success.Should().BeTrue();
            statusResult.NewIsActive.Should().BeTrue();
            statusResult.Message.Should().Be("Sensor status updated.");
        }

        [Fact]
        public async Task ToggleSensorStatus_SensorDoesNotExist_ShouldReturnFailureJson()
        {
            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync("invalid_id")).ReturnsAsync((Sensor)null);

            var result = await _controller.ToggleSensorStatus("invalid_id", true);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var statusResult = jsonResult.Value.Should().BeOfType<AdminController.SensorStatusResult>().Subject;
            statusResult.Success.Should().BeFalse();
            statusResult.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task DeleteSensor_SensorExists_ShouldDeleteAndReturnSuccessJson()
        {
            var sensor = new Sensor { SensorID = "SENSOR_123" };
            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync(sensor.SensorID)).ReturnsAsync(sensor);
            _mockSensorRepo.Setup(r => r.DeleteSensorAsync(sensor.SensorID)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteSensor(sensor.SensorID);

            _mockSensorRepo.Verify(r => r.DeleteSensorAsync(sensor.SensorID), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var statusResult = jsonResult.Value.Should().BeOfType<AdminController.SensorStatusResult>().Subject;
            statusResult.Success.Should().BeTrue();
            statusResult.Message.Should().Be("Sensor deleted successfully!");
        }

        [Fact]
        public async Task DeleteSensor_SensorDoesNotExist_ShouldReturnFailureJson()
        {
            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync("invalid_id")).ReturnsAsync((Sensor)null);

            var result = await _controller.DeleteSensor("invalid_id");

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var statusResult = jsonResult.Value.Should().BeOfType<AdminController.SensorStatusResult>().Subject;
            statusResult.Success.Should().BeFalse();
            statusResult.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task AlertThresholdSettings_ShouldReturnPartialView()
        {
            var settings = new List<AlertThresholdSetting>();
            _mockAlertRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(settings);

            var result = await _controller.AlertThresholdSettings();

            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_AlertThresholdSettingsPartial");
            view.Model.Should().Be(settings);
        }

        [Fact]
        public async Task UpdateAlertThresholds_NullList_ShouldReturnFailureJson()
        {
            var result = await _controller.UpdateAlertThresholds(null);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var alertResult = jsonResult.Value.Should().BeOfType<AdminController.AlertThresholdResult>().Subject;
            alertResult.Success.Should().BeFalse();
            alertResult.Message.Should().Be("No settings to update.");
        }

        [Fact]
        public async Task UpdateAlertThresholds_ValidList_ShouldUpdateAndReturnSuccessJson()
        {
            var settings = new List<AlertThresholdSetting>
            {
                new AlertThresholdSetting { Parameter = "PM2.5", ThresholdValue = 100, IsActive = true }
            };

            _mockAlertRepo.Setup(repo => repo.GetByParameterAsync("PM2.5")).ReturnsAsync(new AlertThresholdSetting { Parameter = "PM2.5" });
            _mockAlertRepo.Setup(repo => repo.UpdateAsync(It.IsAny<AlertThresholdSetting>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateAlertThresholds(settings);

            _mockAlertRepo.Verify(r => r.UpdateAsync(It.IsAny<AlertThresholdSetting>()), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var alertResult = jsonResult.Value.Should().BeOfType<AdminController.AlertThresholdResult>().Subject;
            alertResult.Success.Should().BeTrue();
            alertResult.Message.Should().Be("Alert thresholds updated successfully.");
        }
    }
}
