using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ToolCategoriesController : ControllerBase
    {
        private readonly IToolCategoryService _service;
        public ToolCategoriesController(IToolCategoryService service) => _service = service;

        // GET /api/toolcategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolCategoryListItemDto>>> GetAll(CancellationToken ct)
        {
            var cats = await _service.GetAllAsync(ct);
            var items = cats
                .OrderBy(c => c.Name)
                .Select(c => new ToolCategoryListItemDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive
                })
                .ToList();
            return Ok(items);
        }

        // GET /api/toolcategories/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ToolCategoryDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var c = await _service.GetByIdAsync(id, ct);
            if (c is null) return NotFound();
            return Ok(new ToolCategoryDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            });
        }

        // POST /api/toolcategories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ToolCategoryCreateDto dto, CancellationToken ct)
        {
            var cats = await _service.GetAllAsync(ct);
            if (cats.Any(c => string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "Category name must be unique." });

            var entity = new ToolCategory
            {
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim(),
                IsActive = dto.IsActive
            };
            var ok = await _service.AddAsync(entity, ct);
            if (!ok) return BadRequest(new { message = "Create failed" });

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
            var entity = await _service.GetByIdAsync(id, ct);
            if (entity is null) return NotFound();

            var cats = await _service.GetAllAsync(ct);
            if (cats.Any(c => c.Id != id && string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase)))
                return Conflict(new { message = "Category name must be unique." });

            entity.Name = dto.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim();
            entity.IsActive = dto.IsActive;

            var ok = await _service.UpdateAsync(entity, ct);
            if (!ok) return BadRequest(new { message = "Update failed" });
            return NoContent();
        }

        // DELETE /api/toolcategories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var entity = await _service.GetByIdAsync(id, ct);
            if (entity is null) return NotFound();

            if (entity.Tools.Any())
                return BadRequest(new { message = "Cannot delete category with tools." });

            var ok = await _service.DeleteAsync(id, ct);
            if (!ok) return BadRequest(new { message = "Delete failed" });
            return NoContent();
        }
    }
}
