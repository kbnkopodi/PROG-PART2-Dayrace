using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Tests
{
    public static class TestHelper
    {
        // Fresh empty database for every test
        public static RaceDayContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new RaceDayContext(options);
        }

        // Pretends a user is logged in with the given id and role
        public static T WithUser<T>(this T controller, string userId, string role) where T : ControllerBase
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");

            // Mock the auth service so HttpContext.SignInAsync works in AuthController
            var authService = new Mock<IAuthenticationService>();
            authService.Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection()
                .AddSingleton(authService.Object)
                .BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity),
                    RequestServices = services
                }
            };
            return controller;
        }

        // For anonymous controllers (register/login)
        public static T Anonymous<T>(this T controller) where T : ControllerBase
        {
            var authService = new Mock<IAuthenticationService>();
            authService.Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection()
                .AddSingleton(authService.Object)
                .BuildServiceProvider();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            };
            return controller;
        }

        public static async Task<User> AddUser(RaceDayContext ctx, string id, string role, string password = "Pass123!")
        {
            var user = new User
            {
                UserId = id,
                FullName = $"Test {id}",
                Email = $"{id}@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            return user;
        }

        public static async Task<Event> AddEvent(RaceDayContext ctx, string id, string organiserId)
        {
            var ev = new Event
            {
                EventId = id,
                UserId = organiserId,
                EventName = "Test Event",
                Description = "Desc",
                EventType = "Run",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Johannesburg",
                Distance = 10
            };
            ctx.Events.Add(ev);
            await ctx.SaveChangesAsync();
            return ev;
        }

        public static async Task<Category> AddCategory(RaceDayContext ctx, string id, string eventId)
        {
            var cat = new Category
            {
                CategoryId = id,
                EventId = eventId,
                CategoryName = "10km",
                Distance = 10,
                EntryFee = 200
            };
            ctx.Categories.Add(cat);
            await ctx.SaveChangesAsync();
            return cat;
        }
    }
}