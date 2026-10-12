using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Models;

namespace RaceDay.Tests
{
    public class EventTests
    {
        private static EventDto ValidDto() => new()
        {
            EventName = "Joburg 10km",
            Description = "City run",
            EventType = "Run",
            EventDate = DateTime.UtcNow.AddDays(20),
            Location = "Johannesburg",
            Distance = 10
        };

        [Fact]
        public async Task Create_AsOrganiser_ReturnsCreatedAndSavesEvent()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            var controller = new EventsController(ctx).WithUser("org1", "Organiser");

            var result = await controller.Create(ValidDto());

            Assert.IsType<CreatedAtActionResult>(result);
            var saved = await ctx.Events.SingleAsync();
            Assert.Equal("org1", saved.UserId);
        }

        [Fact]
        public async Task Create_InvalidEventType_ReturnsBadRequest()
        {
            using var ctx = TestHelper.CreateContext();
            var controller = new EventsController(ctx).WithUser("org1", "Organiser");
            var dto = ValidDto();
            dto.EventType = "Swim";

            var result = await controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, await ctx.Events.CountAsync());
        }

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            await TestHelper.AddEvent(ctx, "e1", "org1");
            await TestHelper.AddEvent(ctx, "e2", "org1");
            var controller = new EventsController(ctx).WithUser("p1", "Participant");

            var result = await controller.GetAll();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Update_OwnEvent_ReturnsOk()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            await TestHelper.AddEvent(ctx, "e1", "org1");
            var controller = new EventsController(ctx).WithUser("org1", "Organiser");
            var dto = ValidDto();
            dto.EventName = "Renamed";

            var result = await controller.Update("e1", dto);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Renamed", (await ctx.Events.FindAsync("e1"))!.EventName);
        }

        [Fact]
        public async Task Update_AnotherOrganisersEvent_ReturnsForbid()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            await TestHelper.AddUser(ctx, "org2", "Organiser");
            await TestHelper.AddEvent(ctx, "e1", "org1");
            var controller = new EventsController(ctx).WithUser("org2", "Organiser");

            var result = await controller.Update("e1", ValidDto());

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Update_UnknownEvent_ReturnsNotFound()
        {
            using var ctx = TestHelper.CreateContext();
            var controller = new EventsController(ctx).WithUser("org1", "Organiser");

            var result = await controller.Update("missing", ValidDto());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_OwnEvent_ReturnsNoContent()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            await TestHelper.AddEvent(ctx, "e1", "org1");
            var controller = new EventsController(ctx).WithUser("org1", "Organiser");

            var result = await controller.Delete("e1");

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await ctx.Events.CountAsync());
        }

        [Fact]
        public async Task Delete_AnotherOrganisersEvent_ReturnsForbid()
        {
            using var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            await TestHelper.AddEvent(ctx, "e1", "org1");
            var controller = new EventsController(ctx).WithUser("org2", "Organiser");

            var result = await controller.Delete("e1");

            Assert.IsType<ForbidResult>(result);
            Assert.Equal(1, await ctx.Events.CountAsync());
        }
    }
}