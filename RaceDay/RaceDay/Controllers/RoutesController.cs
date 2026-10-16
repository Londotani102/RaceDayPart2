using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    /// <summary>Race routes.</summary>
    [Route("api/routes")]
    [ApiController]
    [Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public RoutesController(RaceDayDbContext context) => _context = context;

        /// <summary>View all routes. Any authenticated user.</summary>
        [HttpGet]
        public async Task<IActionResult> GetRoutes() =>
            Ok(await _context.Routes.ToListAsync());

        /// <summary>View a specific route. Any authenticated user.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var r = await _context.Routes.FindAsync(id);
            return r == null ? NotFound() : Ok(r);
        }

        /// <summary>Create a route. Organiser only.</summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateRoute([FromBody] RouteCreateRequest request)
        {
            var route = new Routes
            {
                RouteName = request.RouteName,
                Description = request.Description,
                Distance = request.Distance,
                StartLocation = request.StartLocation,
                EndLocation = request.EndLocation,
                RouteMapUrl = request.RouteMapUrl
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoute), new { id = route.RouteId }, route);
        }

        /// <summary>Update a route. Organiser only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] RouteCreateRequest request)
        {
            var existing = await _context.Routes.FindAsync(id);
            if (existing == null) return NotFound();

            existing.RouteName = request.RouteName;
            existing.Description = request.Description;
            existing.Distance = request.Distance;
            existing.StartLocation = request.StartLocation;
            existing.EndLocation = request.EndLocation;
            existing.RouteMapUrl = request.RouteMapUrl;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        /// <summary>Delete a route. Organiser only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null) return NotFound();

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}