using Microsoft.AspNetCore.Authorization;
using RaceDay.Controllers;
using System.Reflection;

namespace RaceDay.Tests
{
    public class RoleAccessTests
    {
        private static string? RolesOn(Type controller, string method)
        {
            var m = controller.GetMethod(method)!;
            return m.GetCustomAttribute<AuthorizeAttribute>()?.Roles;
        }

        [Theory]
        [InlineData(typeof(EventsController))]
        [InlineData(typeof(CategoriesController))]
        [InlineData(typeof(EntriesController))]
        [InlineData(typeof(ResultsController))]
        [InlineData(typeof(UsersController))]
        public void ProtectedControllers_RequireAuthentication(Type controller)
        {
            Assert.NotNull(controller.GetCustomAttribute<AuthorizeAttribute>());
        }

        [Theory]
        [InlineData(typeof(EventsController), "Create")]
        [InlineData(typeof(EventsController), "Update")]
        [InlineData(typeof(EventsController), "Delete")]
        [InlineData(typeof(CategoriesController), "Create")]
        [InlineData(typeof(CategoriesController), "Update")]
        [InlineData(typeof(CategoriesController), "Delete")]
        [InlineData(typeof(EntriesController), "GetForEvent")]
        [InlineData(typeof(ResultsController), "Create")]
        [InlineData(typeof(ResultsController), "Update")]
        [InlineData(typeof(ResultsController), "GetForEvent")]
        public void OrganiserActions_AreRestrictedToOrganisers(Type controller, string method)
        {
            Assert.Equal("Organiser", RolesOn(controller, method));
        }

        [Theory]
        [InlineData(typeof(EntriesController), "Enter")]
        [InlineData(typeof(EntriesController), "GetMyEntries")]
        [InlineData(typeof(EntriesController), "Cancel")]
        [InlineData(typeof(ResultsController), "GetMyResults")]
        public void ParticipantActions_AreRestrictedToParticipants(Type controller, string method)
        {
            Assert.Equal("Participant", RolesOn(controller, method));
        }

        [Fact]
        public void AuthController_IsPublic()
        {
            Assert.Null(typeof(AuthController).GetCustomAttribute<AuthorizeAttribute>());
        }
    }
}