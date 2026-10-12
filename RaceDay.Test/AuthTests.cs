using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Models;

namespace RaceDay.Tests
{
    public class AuthTests
    {
        [Fact]
        public async Task Register_ValidParticipant_ReturnsOk()
        {
            using var ctx = TestHelper.CreateContext();
            var controller = new AuthController(ctx).Anonymous();

            var result = await controller.Register(new RegisterDto
            {
                FullName = "Jane",
                Email = "jane@test.com",
                Password = "Pass123!",
                Role = "Participant"
            });

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1, await ctx.Users.CountAsync());
        }

        [Fact]
        public async Task Register_StoresHashedPassword_NotPlainText()
        {
            using var ctx = TestHelper.CreateContext();
            var controller = new AuthController(ctx).Anonymous();

            await controller.Register(new RegisterDto
            {
                FullName = "Jane",
                Email = "jane@test.com",
                Password = "Pass123!",
                Role = "Organiser"
            });

            var saved = await ctx.Users.FirstAsync();
            Assert.NotEqual("Pass123!", saved.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("Pass123!", saved.PasswordHash));
        }

        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            using var ctx = TestHelper.CreateContext();
            var controller = new AuthController(ctx).Anonymous();

            var result = await controller.Register(new RegisterDto
            {
                FullName = "Eve",
                Email = "eve@test.com",
                Password = "Pass123!",
                Role = "Admin"
            });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, await ctx.Users.CountAsync());
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "u1", "Participant");
            var controller = new AuthController(ctx).Anonymous();

            var result = await controller.Register(new RegisterDto
            {
                FullName = "Copy",
                Email = "u1@test.com",
                Password = "Pass123!",
                Role = "Participant"
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "u1", "Organiser", "Pass123!");
            var controller = new AuthController(ctx).Anonymous();

            var result = await controller.Login(new LoginDto { Email = "u1@test.com", Password = "Pass123!" });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "u1", "Organiser", "Pass123!");
            var controller = new AuthController(ctx).Anonymous();

            var result = await controller.Login(new LoginDto { Email = "u1@test.com", Password = "WrongPass" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            using var ctx = TestHelper.CreateContext();
            var controller = new AuthController(ctx).Anonymous();

            var result = await controller.Login(new LoginDto { Email = "nobody@test.com", Password = "x" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}