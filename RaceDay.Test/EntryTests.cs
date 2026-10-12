using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Models;

namespace RaceDay.Tests
{
    public class EntryTests
    {
        private static async Task<RaceDayContext_> Seed()
        {
            var ctx = TestHelper.CreateContext();
            await TestHelper.AddUser(ctx, "org1", "Organiser");
            await TestHelper.AddUser(ctx, "p1", "Participant");
            await TestHelper.AddEvent(ctx, "e1", "org1");
            await TestHelper.AddCategory(ctx, "c1", "e1");
            return new RaceDayContext_(ctx);
        }

        // tiny wrapper so "using var" disposes the real context
        private sealed class RaceDayContext_ : IDisposable
        {
            public Data.RaceDayContext Ctx { get; }
            public RaceDayContext_(Data.RaceDayContext ctx) => Ctx = ctx;
            public void Dispose() => Ctx.Dispose();
        }

        [Fact]
        public async Task Enter_ValidCategory_RecordsParticipantAndCategory()
        {
            using var db = await Seed();
            var controller = new EntriesController(db.Ctx).WithUser("p1", "Participant");

            var result = await controller.Enter(new EntryDto { CategoryId = "c1" });

            Assert.IsType<CreatedAtActionResult>(result);
            var entry = await db.Ctx.Entries.SingleAsync();
            Assert.Equal("p1", entry.UserId);       // from the session
            Assert.Equal("c1", entry.CategoryId);   // the chosen category
            Assert.Equal("Registered", entry.Status);
        }

        [Fact]
        public async Task Enter_UnknownCategory_ReturnsNotFound()
        {
            using var db = await Seed();
            var controller = new EntriesController(db.Ctx).WithUser("p1", "Participant");

            var result = await controller.Enter(new EntryDto { CategoryId = "nope" });

            Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(0, await db.Ctx.Entries.CountAsync());
        }

        [Fact]
        public async Task Enter_SameEventTwice_ReturnsBadRequest()
        {
            using var db = await Seed();
            var controller = new EntriesController(db.Ctx).WithUser("p1", "Participant");

            await controller.Enter(new EntryDto { CategoryId = "c1" });
            var second = await controller.Enter(new EntryDto { CategoryId = "c1" });

            Assert.IsType<BadRequestObjectResult>(second);
            Assert.Equal(1, await db.Ctx.Entries.CountAsync());
        }

        [Fact]
        public async Task GetForEvent_OwningOrganiser_ReturnsEntries()
        {
            using var db = await Seed();
            await new EntriesController(db.Ctx).WithUser("p1", "Participant")
                .Enter(new EntryDto { CategoryId = "c1" });
            var controller = new EntriesController(db.Ctx).WithUser("org1", "Organiser");

            var result = await controller.GetForEvent("e1");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetForEvent_DifferentOrganiser_ReturnsForbid()
        {
            using var db = await Seed();
            await TestHelper.AddUser(db.Ctx, "org2", "Organiser");
            var controller = new EntriesController(db.Ctx).WithUser("org2", "Organiser");

            var result = await controller.GetForEvent("e1");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Cancel_AnotherParticipantsEntry_ReturnsForbid()
        {
            using var db = await Seed();
            await TestHelper.AddUser(db.Ctx, "p2", "Participant");
            await new EntriesController(db.Ctx).WithUser("p1", "Participant")
                .Enter(new EntryDto { CategoryId = "c1" });
            var entryId = (await db.Ctx.Entries.SingleAsync()).EntryId;
            var controller = new EntriesController(db.Ctx).WithUser("p2", "Participant");

            var result = await controller.Cancel(entryId);

            Assert.IsType<ForbidResult>(result);
        }
    }
}