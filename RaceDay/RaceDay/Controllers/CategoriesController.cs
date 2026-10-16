using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    /// <summary>Race event categories.</summary>
    [Route("api/categories")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public CategoriesController(RaceDayDbContext context) => _context = context;

        /// <summary>View all categories. Any authenticated user.</summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories() =>
            Ok(await _context.Categories.ToListAsync());

        /// <summary>View a specific category. Any authenticated user.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            return cat == null ? NotFound() : Ok(cat);
        }

        /// <summary>Create a category. Organiser only.</summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateRequest request)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == request.EventId);
            if (!eventExists) return BadRequest("Event not found.");

            var cat = new Categories
            {
                CategoryName = request.CategoryName,
                MinAge = request.MinAge,
                MaxAge = request.MaxAge,
                Distance = request.Distance,
                EventId = request.EventId
            };

            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategory), new { id = cat.CategoryId }, cat);
        }

        /// <summary>Update a category. Organiser only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryCreateRequest request)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return NotFound();

            existing.CategoryName = request.CategoryName;
            existing.MinAge = request.MinAge;
            existing.MaxAge = request.MaxAge;
            existing.Distance = request.Distance;
            existing.EventId = request.EventId;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        /// <summary>Delete a category. Organiser only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}