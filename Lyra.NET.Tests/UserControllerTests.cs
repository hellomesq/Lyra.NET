using Lyra.Controllers;
using Lyra.Data;
using Lyra.DTOs;
using Lyra.Models;
using Lyra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lyra.NET.Tests
{
    public class UserControllerTests
    {
        private AppDbContext CreateInMemoryDatabase()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // banco novo para cada teste
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateUser_DeveCriarUsuarioERetornarCreated()
            var db = CreateInMemoryDatabase();

            var mockService = new Mock<UserService>(db);

            mockService
                .Setup(s => s.InserirUsuario(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(1);

            var logger = Mock.Of<ILogger<UserController>>();

            var controller = new UserController(mockService.Object, db, logger);

            var dto = new UserDto
            {
                Name = "Hellen",
                Email = "hellen@test.com",
                Password = "123456",
                Experience_Level = "Junior"
            };

            var result = await controller.CreateUser(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);

            Assert.Equal(1, db.Users.Count());
        }
    }
}
