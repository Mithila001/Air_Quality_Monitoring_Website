using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SDTP_Project1.Controllers;
using SDTP_Project1.Models;
using SDTP_Project1.Repositories;
using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;


namespace AQISystemUnit.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ISystemAdminRepository> _mockUserRepo;
        private readonly Mock<IPasswordHasher<AdminUser>> _mockPasswordHasher;
        private readonly AuthController _controller;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IAuthenticationService> _mockAuthService;

        public AuthControllerTests()
        {
            _mockUserRepo = new Mock<ISystemAdminRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher<AdminUser>>();

            _controller = new AuthController(_mockUserRepo.Object, _mockPasswordHasher.Object);

            // Required mocks
            var tempData = new Mock<ITempDataDictionary>();
            var tempDataFactory = new Mock<ITempDataDictionaryFactory>();
            tempDataFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(tempData.Object);

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);

            var httpContext = new DefaultHttpContext();

            // Mock Authentication service
            var authService = new Mock<IAuthenticationService>();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                           .Returns(authService.Object);
            httpContext.RequestServices = serviceProvider.Object;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller.TempData = tempDataFactory.Object.GetTempData(httpContext);
            _controller.Url = urlHelper.Object;
        }

        [Fact]
        public async Task Login_ValidCredentials_SystemAdmin_ShouldRedirectToSystemAdmin()
        {
            var user = new AdminUser
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@test.com",
                PasswordHash = "hashed",
                UserRole = "System Admin"
            };

            _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AdminUser> { user });
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, "password"))
                               .Returns(PasswordVerificationResult.Success);

            var result = await _controller.Login("admin@test.com", "password");

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("SystemAdmin");
        }

        [Fact]
        public async Task Login_InvalidEmail_ShouldReturnViewWithError()
        {
            _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AdminUser>());

            var result = await _controller.Login("notfound@test.com", "password");

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Login_WrongPassword_ShouldReturnViewWithError()
        {
            var user = new AdminUser
            {
                Id = 1,
                Name = "Admin",
                Email = "admin@test.com",
                PasswordHash = "hashed",
                UserRole = "System Admin"
            };

            _mockUserRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AdminUser> { user });
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, "wrongpass"))
                               .Returns(PasswordVerificationResult.Failed);

            var result = await _controller.Login("admin@test.com", "wrongpass");

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Logout_ShouldRedirectToHomeIndex()
        {
            var result = await _controller.Logout();

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            redirectResult.ControllerName.Should().Be("Home");
        }
    }
}
