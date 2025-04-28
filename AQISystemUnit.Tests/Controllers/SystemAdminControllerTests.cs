// SystemAdminControllerTests.cs - FIXED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using SDTP_Project1.Controllers;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using Xunit;

namespace AQISystemUnit.Tests.Controllers
{
    public class SystemAdminControllerTests
    {
        private readonly Mock<ISystemAdminRepository> _mockAdminRepo;
        private readonly Mock<ISensorRepository> _mockSensorRepo;
        private readonly Mock<IPasswordHasher<AdminUser>> _mockPasswordHasher;
        private readonly SystemAdminController _controller;

        public SystemAdminControllerTests()
        {
            _mockAdminRepo = new Mock<ISystemAdminRepository>();
            _mockSensorRepo = new Mock<ISensorRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher<AdminUser>>();

            _controller = new SystemAdminController(
                _mockAdminRepo.Object,
                _mockSensorRepo.Object,
                _mockPasswordHasher.Object
            );

            // Setup TempData for controller
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            // Create and set up HttpContext for TryUpdateModelAsync to work
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithAdminUsers()
        {
            // Arrange
            var users = new List<AdminUser> { new AdminUser(), new AdminUser() };
            var sensors = new List<Sensor> { new Sensor(), new Sensor() };

            _mockAdminRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            _mockSensorRepo.Setup(r => r.GetAllSensorsAsync()).ReturnsAsync(sensors);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task EditAdmin_UserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _mockAdminRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((AdminUser)null);

            // Act
            var result = await _controller.EditAdmin(1, null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task EditAdmin_UpdateFails_ShouldRedirectToIndexWithError()
        {
            // Arrange
            var user = new AdminUser { Id = 1, Name = "Test User", Email = "test@test.com" };
            _mockAdminRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _mockAdminRepo.Setup(r => r.UpdateAsync(It.IsAny<AdminUser>())).ThrowsAsync(new Exception("DB Error"));

            // Need to mock TryUpdateModelAsync behavior since we can't fully set up metadata provider
            var controllerMock = new Mock<SystemAdminController>(
                _mockAdminRepo.Object,
                _mockSensorRepo.Object,
                _mockPasswordHasher.Object)
            { CallBase = true };

            // Set TempData on the mock controller
            controllerMock.Object.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            // Mock the TryUpdateModelAsync method to return true (success)
            controllerMock
                .Setup(c => c.TryUpdateModelAsync(
                    It.IsAny<AdminUser>(),
                    It.IsAny<string>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<AdminUser, object>>[]>()))
                .ReturnsAsync(true);

            // Act
            var result = await controllerMock.Object.EditAdmin(1, null);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            controllerMock.Object.TempData.Should().ContainKey("ErrorMessage");
        }

        [Fact]
        public async Task DeleteAdmin_ValidId_ShouldRedirectToIndex()
        {
            // Arrange
            var user = new AdminUser { Id = 1 };
            _mockAdminRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _mockAdminRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAdmin(1);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            _controller.TempData.Should().ContainKey("SuccessMessage");
        }

        [Fact]
        public async Task DeleteAdmin_UserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _mockAdminRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AdminUser)null);

            // Act
            var result = await _controller.DeleteAdmin(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task AddAdmin_InvalidModel_ShouldReturnPartialView()
        {
            // Arrange
            var newAdmin = new AdminUser { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.AddAdmin(newAdmin);

            // Assert
            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_addNewAdmin");
            view.Model.Should().Be(newAdmin);
        }

        [Fact]
        public async Task AddAdmin_EmailAlreadyExists_ShouldReturnPartialView()
        {
            // Arrange
            var newAdmin = new AdminUser
            {
                Name = "Test",
                Email = "test@example.com",
                Gender = "Male",
                UserRole = "User Admin",
                PasswordHash = "test"
            };
            var existingUsers = new List<AdminUser> { new AdminUser { Email = "test@example.com" } };

            _mockAdminRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(existingUsers);

            // Act
            var result = await _controller.AddAdmin(newAdmin);

            // Assert
            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_addNewAdmin");
            view.Model.Should().Be(newAdmin);
        }

        [Fact]
        public async Task AddAdmin_ValidAdmin_ShouldRedirectToIndex()
        {
            // Arrange
            var newAdmin = new AdminUser
            {
                Name = "Admin",
                Email = "admin@test.com",
                Gender = "Male",
                UserRole = "User Admin",
                PasswordHash = "test"
            };

            _mockAdminRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AdminUser>());
            _mockAdminRepo.Setup(r => r.AddAsync(It.IsAny<AdminUser>())).Returns(Task.CompletedTask);
            _mockPasswordHasher.Setup(h => h.HashPassword(It.IsAny<AdminUser>(), It.IsAny<string>())).Returns("hashedpassword");

            // Act
            var result = await _controller.AddAdmin(newAdmin);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
            _controller.TempData.Should().ContainKey("NewAdminPassword");
            _controller.TempData.Should().ContainKey("SuccessMessage");
        }
    }
}