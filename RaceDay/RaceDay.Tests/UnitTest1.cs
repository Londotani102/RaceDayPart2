using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Controllers;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Tests
{
    public class EventsControllerTests
    {
        // Helper: fresh in-memory database per test
        private RaceDayDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        // Helper: simulate an authenticated user
        private void SetUser(EventsController controller, int userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        // ============ GET ALL ============
        [Fact]
        public async Task GetEvents_ReturnsAllEvents()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Events.Add(new Event
            {
                EventId = 1,
                EventName = "Cape Town Marathon",
                EventDate = new DateTime(2026, 3, 15),
                description = "Annual marathon",
                Location = "Cape Town",
                EventType = "Run",
                RouteMapUrl = "https://example.com/map",
                RouteId = 1,
                OrganiserId = 1
            });
            context.Events.Add(new Event
            {
                EventId = 2,
                EventName = "Joburg 10K",
                EventDate = new DateTime(2026, 4, 20),
                description = "10km race",
                Location = "Joburg",
                EventType = "Run",
                RouteMapUrl = "https://example.com/map2",
                RouteId = 2,
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUser(controller, 1, "Participant");

            // Act
            var result = await controller.GetEvents();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<Event>>(ok.Value).ToList();
            Assert.Equal(2, list.Count);
        }

        // ============ GET BY ID ============
        [Fact]
        public async Task GetEvent_ReturnsEvent_WhenExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Events.Add(new Event
            {
                EventId = 1,
                EventName = "Cape Town Marathon",
                EventDate = new DateTime(2026, 3, 15),
                description = "Annual",
                Location = "Cape Town",
                EventType = "Run",
                RouteMapUrl = "",
                RouteId = 1,
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUser(controller, 1, "Participant");

            // Act
            var result = await controller.GetEvent(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<Event>(ok.Value);
            Assert.Equal("Cape Town Marathon", returned.EventName);
        }

        [Fact]
        public async Task GetEvent_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new EventsController(context);
            SetUser(controller, 1, "Participant");

            // Act
            var result = await controller.GetEvent(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ============ POST ============
        [Fact]
        public async Task CreateEvent_AsOrganiser_PersistsEvent()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new EventsController(context);
            SetUser(controller, 1, "Organiser");

            var request = new EventCreateRequest
            {
                EventName = "New Race",
                description = "Test",
                EventDate = new DateTime(2026, 5, 1),
                Location = "Joburg",
                EventType = "Run",
                RouteMapUrl = "https://example.com",
                RouteId = 1
            };

            // Act
            var result = await controller.CreateEvent(request);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);

            var saved = await context.Events.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("New Race", saved!.EventName);
            Assert.Equal(1, saved.OrganiserId); // taken from token, not body
        }

        // ============ PUT ============
        [Fact]
        public async Task UpdateEvent_OwningOrganiser_UpdatesEvent()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Events.Add(new Event
            {
                EventId = 1,
                EventName = "Old Name",
                EventDate = new DateTime(2026, 3, 15),
                description = "",
                Location = "Cape Town",
                EventType = "Run",
                RouteMapUrl = "",
                RouteId = 1,
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUser(controller, 1, "Organiser");

            var request = new EventCreateRequest
            {
                EventName = "Updated Name",
                description = "Updated",
                EventDate = new DateTime(2026, 3, 15),
                Location = "Cape Town",
                EventType = "Run",
                RouteMapUrl = "",
                RouteId = 1
            };

            // Act
            var result = await controller.UpdateEvent(1, request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var updated = await context.Events.FindAsync(1);
            Assert.Equal("Updated Name", updated!.EventName);
        }

        [Fact]
        public async Task UpdateEvent_OtherOrganiser_ReturnsForbid()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Events.Add(new Event
            {
                EventId = 1,
                EventName = "Sarah's Event",
                EventDate = new DateTime(2026, 3, 15),
                description = "",
                Location = "",
                EventType = "",
                RouteMapUrl = "",
                RouteId = 1,
                OrganiserId = 1   // owned by organiser 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUser(controller, 999, "Organiser"); // different organiser

            var request = new EventCreateRequest
            {
                EventName = "Hijack",
                description = "",
                EventDate = DateTime.UtcNow,
                Location = "",
                EventType = "",
                RouteMapUrl = "",
                RouteId = 1
            };

            // Act
            var result = await controller.UpdateEvent(1, request);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new EventsController(context);
            SetUser(controller, 1, "Organiser");

            var request = new EventCreateRequest
            {
                EventName = "X",
                description = "",
                EventDate = DateTime.UtcNow,
                Location = "",
                EventType = "",
                RouteMapUrl = "",
                RouteId = 1
            };

            // Act
            var result = await controller.UpdateEvent(999, request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ============ DELETE ============
        [Fact]
        public async Task DeleteEvent_OwningOrganiser_RemovesEvent()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Events.Add(new Event
            {
                EventId = 1,
                EventName = "To Delete",
                EventDate = DateTime.UtcNow,
                description = "",
                Location = "",
                EventType = "",
                RouteMapUrl = "",
                RouteId = 1,
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUser(controller, 1, "Organiser");

            // Act
            var result = await controller.DeleteEvent(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var ev = await context.Events.FindAsync(1);
            Assert.Null(ev);
        }

        [Fact]
        public async Task DeleteEvent_OtherOrganiser_ReturnsForbid()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Events.Add(new Event
            {
                EventId = 1,
                EventName = "Protected",
                EventDate = DateTime.UtcNow,
                description = "",
                Location = "",
                EventType = "",
                RouteMapUrl = "",
                RouteId = 1,
                OrganiserId = 1
            });
            await context.SaveChangesAsync();

            var controller = new EventsController(context);
            SetUser(controller, 999, "Organiser");

            // Act
            var result = await controller.DeleteEvent(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
            var ev = await context.Events.FindAsync(1);
            Assert.NotNull(ev); // still there
        }

        [Fact]
        public async Task DeleteEvent_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new EventsController(context);
            SetUser(controller, 1, "Organiser");

            // Act
            var result = await controller.DeleteEvent(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}