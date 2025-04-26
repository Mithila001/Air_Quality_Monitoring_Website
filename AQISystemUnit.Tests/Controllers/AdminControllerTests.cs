using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SDTP_Project1.Controllers;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using SDTP_Project1.Services;
using Xunit;

namespace AQISystemUnit.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<ISensorRepository> _mockSensorRepo;
        private readonly Mock<IAlertThresholdSettingRepository> _mockAlertRepo;
        private readonly Mock<ISensorService> _mockSensorService;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockSensorRepo = new Mock<ISensorRepository>();
            _mockAlertRepo = new Mock<IAlertThresholdSettingRepository>();
            _mockSensorService = new Mock<ISensorService>();
            _controller = new AdminController(_mockSensorRepo.Object, _mockAlertRepo.Object, _mockSensorService.Object);
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithSensorsAndAverageAQI()
        {
            var sensors = new List<Sensor> { new Sensor(), new Sensor() };
            _mockSensorRepo.Setup(repo => repo.GetAllSensorsAsync()).ReturnsAsync(sensors);
            _mockSensorService.Setup(service => service.GetAverageAQILast30DaysForAllSensors()).ReturnsAsync(42.0);

            var result = await _controller.Index();

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(sensors);
            ((double)_controller.ViewBag.AverageAQI).Should().Be(42.0);
        }

        [Fact]
        public void CreateSensor_Get_ShouldReturnView()
        {
            // Act
            var result = _controller.CreateSensor();

            // Assert
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
            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().Be(sensor);
        }

        [Fact]
        public async Task CreateSensor_Post_ValidModel_ShouldSaveAndRedirect()
        {
            var sensor = new Sensor { City = "Colombo" };
            _mockSensorRepo.Setup(repo => repo.AddSensorAsync(It.IsAny<Sensor>())).Returns(Task.CompletedTask);

            var result = await _controller.CreateSensor(sensor);

            _mockSensorRepo.Verify(repo => repo.AddSensorAsync(It.IsAny<Sensor>()), Times.Once);
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }


        [Fact]
        public async Task EditSensor_Get_SensorExists_ShouldReturnPartialView()
        {
            // Arrange
            var sensor = new Sensor { SensorID = "S_CO_202404261200" };
            _mockSensorRepo.Setup(r => r.GetSensorByIdAsync(sensor.SensorID)).ReturnsAsync(sensor);

            // Act
            var result = await _controller.EditSensor(sensor.SensorID);

            // Assert
            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_EditSensorPartial");
            view.Model.Should().Be(sensor);
        }

        [Fact]
        public async Task EditSensor_Get_SensorDoesNotExist_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.EditSensor("nonexistent");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task EditSensor_Post_InvalidModel_ShouldReturnPartialView()
        {
            // Arrange
            var sensor = new Sensor { SensorID = "S_CO_202404261200" };
            _controller.ModelState.AddModelError("City", "Required");

            // Act
            var result = await _controller.EditSensor(sensor);

            // Assert
            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_EditSensorPartial");
            view.Model.Should().Be(sensor);
        }

        [Fact]
        public async Task EditSensor_Post_ValidModel_ShouldUpdateAndReturnJson()
        {
            var sensor = new Sensor { SensorID = "S_ID", City = "City" };
            var existing = new Sensor { SensorID = "S_ID" };
            _mockSensorRepo.Setup(repo => repo.GetSensorByIdAsync("S_ID")).ReturnsAsync(existing);

            var result = await _controller.EditSensor(sensor);

            _mockSensorRepo.Verify(repo => repo.UpdateSensorAsync(It.IsAny<Sensor>()), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task ToggleSensorStatus_SensorExists_ShouldUpdateAndReturnJson()
        {
            var sensor = new Sensor { SensorID = "S_ID", IsActive = false };
            _mockSensorRepo.Setup(repo => repo.GetSensorByIdAsync("S_ID")).ReturnsAsync(sensor);

            var result = await _controller.ToggleSensorStatus("S_ID", true);

            _mockSensorRepo.Verify(repo => repo.UpdateSensorAsync(sensor), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, newIsActive = true });
        }

        [Fact]
        public async Task ToggleSensorStatus_SensorDoesNotExist_ShouldReturnFailureJson()
        {
            _mockSensorRepo.Setup(repo => repo.GetSensorByIdAsync("S_ID")).ReturnsAsync((Sensor)null);

            var result = await _controller.ToggleSensorStatus("S_ID", true);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, message = "Sensor not found." });
        }

        [Fact]
        public async Task DeleteSensor_SensorExists_ShouldDeleteAndReturnSuccessJson()
        {
            var sensor = new Sensor { SensorID = "S_ID" };
            _mockSensorRepo.Setup(repo => repo.GetSensorByIdAsync("S_ID")).ReturnsAsync(sensor);

            var result = await _controller.DeleteSensor("S_ID");

            _mockSensorRepo.Verify(repo => repo.DeleteSensorAsync("S_ID"), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true, message = "Sensor deleted successfully!" });
        }

        [Fact]
        public async Task DeleteSensor_SensorDoesNotExist_ShouldReturnFailureJson()
        {
            _mockSensorRepo.Setup(repo => repo.GetSensorByIdAsync("S_ID")).ReturnsAsync((Sensor)null);

            var result = await _controller.DeleteSensor("S_ID");

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, message = "Sensor not found." });
        }

        [Fact]
        public async Task AlertThresholdSettings_ShouldReturnPartialView()
        {
            // Arrange
            var settings = new List<AlertThresholdSetting>();
            _mockAlertRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(settings);

            // Act
            var result = await _controller.AlertThresholdSettings();

            // Assert
            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_AlertThresholdSettingsPartial");
            view.Model.Should().Be(settings);
        }

        [Fact]
        public async Task UpdateAlertThresholds_NullList_ShouldReturnFailureJson()
        {
            var result = await _controller.UpdateAlertThresholds(null);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = false, message = "No settings to update." });
        }

        [Fact]
        public async Task UpdateAlertThresholds_ValidList_ShouldUpdateAndReturnSuccessJson()
        {
            var settings = new List<AlertThresholdSetting>
            {
                new AlertThresholdSetting { Parameter = "PM2.5", ThresholdValue = 100, IsActive = true }
            };
            _mockAlertRepo.Setup(repo => repo.GetByParameterAsync("PM2.5"))
                .ReturnsAsync(new AlertThresholdSetting { Parameter = "PM2.5" });

            var result = await _controller.UpdateAlertThresholds(settings);

            _mockAlertRepo.Verify(repo => repo.UpdateAsync(It.IsAny<AlertThresholdSetting>()), Times.Once);
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeEquivalentTo(new { success = true });
        }
    }
}
