using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Tests
{
    public class EntryTests
    {
        private static async Task<RaceDayContext> Seed()
        {
            var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            await TestHelper.AddUser(ctx, "p1", "Participant");
            await TestHelper.AddEvent(ctx, "e1", "org1");
            await TestHelper.AddCategory(ctx, "c1", "e1");
            return ctx;
        }

        [Fact]
        public async Task Enter_ValidCategory_RecordsParticipantAndCategory()
        {
            using var db = await Seed();
            var controller = new EntriesController(db).WithUser("p1", "Participant");

            var result = await controller.Enter(new EntryDto { CategoryId = "c1" });

            Assert.IsType<CreatedAtActionResult>(result);
            var entry = await db.Entries.SingleAsync();
            Assert.Equal("p1", entry.UserId);
            Assert.Equal("c1", entry.CategoryId);
            Assert.Equal("Registered", entry.Status);
        }

        [Fact]
        public async Task Enter_UnknownCategory_ReturnsNotFound()
        {
            using var db = await Seed();
            var controller = new EntriesController(db).WithUser("p1", "Participant");

            var result = await controller.Enter(new EntryDto { CategoryId = "nope" });

            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(0, await db.Entries.CountAsync());
        }

        [Fact]
        public async Task Enter_SameEventTwice_ReturnsBadRequest()
        {
            using var db = await Seed();
            var controller = new EntriesController(db).WithUser("p1", "Participant");

            await controller.Enter(new EntryDto { CategoryId = "c1" });
            var second = await controller.Enter(new EntryDto { CategoryId = "c1" });

            Assert.IsType<BadRequestObjectResult>(second);
            Assert.Equal(1, await db.Entries.CountAsync());
        }

        [Fact]
        public async Task GetForEvent_OwningOrganiser_ReturnsOk()
        {
            using var db = await Seed();
            await new EntriesController(db).WithUser("p1", "Participant")
                .Enter(new EntryDto { CategoryId = "c1" });
            var controller = new EntriesController(db).WithUser("org1", "Organiser");

            var result = await controller.GetForEvent("e1");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetForEvent_DifferentOrganiser_ReturnsForbid()
        {
            using var db = await Seed();
            await TestHelper.AddUser(db, "org2", "Organiser");
            var controller = new EntriesController(db).WithUser("org2", "Organiser");

            var result = await controller.GetForEvent("e1");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Cancel_AnotherParticipantsEntry_ReturnsForbid()
        {
            using var db = await Seed();
            await TestHelper.AddUser(db, "p2", "Participant");
            await new EntriesController(db).WithUser("p1", "Participant")
                .Enter(new EntryDto { CategoryId = "c1" });
            var entryId = (await db.Entries.SingleAsync()).EntryId;
            var controller = new EntriesController(db).WithUser("p2", "Participant");

            var result = await controller.Cancel(entryId);

            Assert.IsType<ForbidResult>(result);
        }
    }
}