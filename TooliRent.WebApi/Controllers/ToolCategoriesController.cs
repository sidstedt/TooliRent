using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TooliRent.Application.DTOs;
using TooliRent.Domain.Entities;
using TooliRent.Infrastructure.Persistence;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Category CRUD is an admin function per task
    public class ToolCategoriesController : ControllerBase
    {
        private readonly TooliRentDbContext _db;
        public ToolCategoriesController(TooliRentDbContext db) => _db = db;

        // GET /api/toolcategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolCategoryListItemDto>>> GetAll(CancellationToken ct)
        {
            var items = await _db.ToolCategories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new ToolCategoryListItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive
                })
                .ToListAsync(ct);
            return Ok(items);
        }

        // GET /api/toolcategories/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ToolCategoryDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var item = await _db.ToolCategories.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new ToolCategoryDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .FirstOrDefaultAsync(ct);
            return item is null ? NotFound() : Ok(item);
        }

        // POST /api/toolcategories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ToolCategoryCreateDto dto, CancellationToken ct)
        {
            if (await _db.ToolCategories.AnyAsync(c => c.Name == dto.Name, ct))
                return Conflict(new { message = "Category name must be unique." });

            var entity = new ToolCategory
            {
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim(),
                IsActive = dto.IsActive
            };
            _db.ToolCategories.Add(entity);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new ToolCategoryDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive
            });
        }

        // PUT /api/toolcategories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ToolCategoryUpdateDto dto, CancellationToken ct)
        {
            var entity = await _db.ToolCategories.FindAsync(new object[] { id }, ct);
            if (entity is null) return NotFound();

            var exists = await _db.ToolCategories.AnyAsync(c => c.Id != id && c.Name == dto.Name, ct);
            if (exists) return Conflict(new { message = "Category name must be unique." });

            entity.Name = dto.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim();
            entity.IsActive = dto.IsActive;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE /api/toolcategories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var entity = await _db.ToolCategories.Include(c => c.Tools).FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity is null) return NotFound();

            if (entity.Tools.Any())
                return BadRequest(new { message = "Cannot delete category with tools." });

            _db.ToolCategories.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
