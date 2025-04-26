using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SDTP_Project1.Controllers;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using Xunit;

namespace AQISystemUnit.Tests.Controllers
{
    public  class SystemAdminControllerTests
    {
        private readonly Mock<ISystemAdminRepository> _mockAdminRepo;
        private readonly Mock<ISensorRepository> _mockSensorRepo;
        private readonly Mock<IPasswordHasher<AdminUser>> _mockHasher;
        private readonly SystemAdminController _controller;

        public SystemAdminControllerTests()
        {
            _mockAdminRepo = new Mock<ISystemAdminRepository>();
            _mockSensorRepo = new Mock<ISensorRepository>();
            _mockHasher = new Mock<IPasswordHasher<AdminUser>>();
            _controller = new SystemAdminController(_mockAdminRepo.Object, _mockSensorRepo.Object, _mockHasher.Object);
        }

        [Fact]
        public async Task Index_ShouldReturnViewResult_WithCorrectViewBagCounts()
        {
            // Arrange
            var admins = new List<AdminUser>
            {
                new AdminUser { UserRole = "System Admin", IsActive = true },
                new AdminUser { UserRole = "User Admin", IsActive = false }
            };
            var sensors = new List<Sensor>
            {
                new Sensor { IsActive = true },
                new Sensor { IsActive = false }
            };
            _mockAdminRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(admins);
            _mockSensorRepo.Setup(r => r.GetAllSensorsAsync()).ReturnsAsync(sensors);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(admins);
            ((int)_controller.ViewBag.TotalUserAccounts).Should().Be(2);
            ((int)_controller.ViewBag.TotalUserAdmins).Should().Be(1);
            ((int)_controller.ViewBag.TotalSystemAdmins).Should().Be(1);
            ((int)_controller.ViewBag.TotalActiveAdmins).Should().Be(1);
            ((int)_controller.ViewBag.TotalDeactiveAdmins).Should().Be(1);
            ((int)_controller.ViewBag.TotalActiveSensors).Should().Be(1);
            ((int)_controller.ViewBag.TotalDeactivatedSensors).Should().Be(1);
        }

        [Fact]
        public async Task EditAdmin_ValidUpdateWithNewPassword_ShouldUpdateAndRedirect()
        {
            // Arrange
            var updatedUser = new AdminUser
            {
                Id = 1,
                Name = "Updated",
                Email = "email",
                PhoneNumber = "123",
                IsActive = true,
                Gender = "M",
                Age = 30,
                UserRole = "System Admin"
            };

            var existingUser = new AdminUser
            {
                Id = 1,
                Name = "OldName",
                Email = "oldemail",
                PhoneNumber = "321",
                IsActive = false,
                Gender = "F",
                Age = 25,
                UserRole = "User Admin"
            };

            _mockAdminRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingUser);
            _mockHasher.Setup(h => h.HashPassword(existingUser, "newPwd")).Returns("hashedPwd");
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.EditAdmin(updatedUser, "newPwd");

            // Assert
            _mockAdminRepo.Verify(r => r.UpdateAsync(It.IsAny<AdminUser>()), Times.Once);
            existingUser.PasswordHash.Should().Be("hashedPwd");
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }


        [Fact]
        public async Task DeleteAdmin_ShouldCallDeleteAndRedirect()
        {
            // Act
            var result = await _controller.DeleteAdmin(1);

            // Assert
            _mockAdminRepo.Verify(r => r.DeleteAsync(1), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task AddAdmin_InvalidModel_ShouldReturnPartialView()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");
            var admin = new AdminUser();

            // Act
            var result = await _controller.AddAdmin(admin);

            // Assert
            var view = result.Should().BeOfType<PartialViewResult>().Subject;
            view.ViewName.Should().Be("_addNewAdmin");
            view.Model.Should().Be(admin);
        }
    }
}
