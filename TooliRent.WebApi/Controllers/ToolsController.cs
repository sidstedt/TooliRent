using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Interfaces;
using TooliRent.Domain.Queries;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly IToolService _tools;
        private readonly IToolCategoryRepository _categories;
        public ToolsController(IToolService tools, IToolCategoryRepository categories)
        {
            _tools = tools;
            _categories = categories;
        }

        // GET /api/tools?categoryId=&status=&availableOnly=&search
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolListItemDto>>> Get([FromQuery] int? categoryId, [FromQuery] ToolStatus? status, [FromQuery] bool? availableOnly, [FromQuery] string? search, CancellationToken ct)
        {
            var criteria = new ToolSearchCriteria(
                Name: search,
                CategoryId: categoryId,
                Status: status,
                MinPrice: null,
                MaxPrice: null
            );
            var list = await _tools.SearchAsync(criteria, ct);
            if (availableOnly == true)
                list = list.Where(t => t.QuantityAvailable > 0 && t.Status == ToolStatus.Available).ToList();

            var items = list
                .OrderBy(t => t.Id)
                .Select(t => new ToolListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    PricePerDay = t.PricePerDay,
                    QuantityAvailable = t.QuantityAvailable,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    Status = (int)t.Status
                }).ToList();

            return Ok(items);
        }

        // GET /api/tools/{id}
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ToolDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var t = await _tools.GetToolByIdAsync(id, ct);
            if (t is null) return NotFound();
            return Ok(new ToolDetailDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                PricePerDay = t.PricePerDay,
                QuantityAvailable = t.QuantityAvailable,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                Status = (int)t.Status,
                CreatedAt = t.CreatedAt
            });
        }

        // GET /api/tools/available?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD&categoryId
        [AllowAnonymous]
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<ToolListItemDto>>> GetAvailableInPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? categoryId, CancellationToken ct)
        {
            if (startDate.Date >= endDate.Date)
                return BadRequest(new { message = "startDate must be before endDate" });

            var list = await _tools.GetAvailableToolsAsync(startDate, endDate, ct);
            if (categoryId.HasValue)
                list = list.Where(t => t.CategoryId == categoryId.Value).ToList();

            var result = list
                .OrderBy(t => t.Name)
                .Select(t => new ToolListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    PricePerDay = t.PricePerDay,
                    QuantityAvailable = t.QuantityAvailable,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    Status = (int)t.Status
                }).ToList();
            return Ok(result);
        }

        // POST /api/tools
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ToolCreateDto dto, CancellationToken ct)
        {
            var category = await _categories.GetByIdAsync(dto.CategoryId, ct);
            if (category is null) return BadRequest(new { message = "Category not found." });

            var entity = new Tool
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                PricePerDay = dto.PricePerDay,
                QuantityAvailable = dto.QuantityAvailable,
                CategoryId = dto.CategoryId,
                Status = ToolStatus.Available
            };
            await _tools.AddToolAsync(entity, ct);

            var created = await _tools.GetToolByIdAsync(entity.Id, ct);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new ToolDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                PricePerDay = entity.PricePerDay,
                QuantityAvailable = entity.QuantityAvailable,
                CategoryId = entity.CategoryId,
                CategoryName = created?.Category.Name ?? string.Empty,
                Status = (int)entity.Status,
                CreatedAt = entity.CreatedAt
            });
        }

        // PUT /api/tools/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ToolUpdateDto dto, CancellationToken ct)
        {
            var entity = await _tools.GetToolByIdAsync(id, ct);
            if (entity is null) return NotFound();

            var category = await _categories.GetByIdAsync(dto.CategoryId, ct);
            if (category is null) return BadRequest(new { message = "Category not found." });

            if (!Enum.IsDefined(typeof(ToolStatus), dto.Status))
                return BadRequest(new { message = "Invalid status" });

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description.Trim();
            entity.PricePerDay = dto.PricePerDay;
            entity.QuantityAvailable = dto.QuantityAvailable;
            entity.CategoryId = dto.CategoryId;
            entity.Status = (ToolStatus)dto.Status;

            var ok = await _tools.UpdateToolAsync(entity, ct);
            if (!ok) return BadRequest(new { message = "Update failed" });
            return NoContent();
        }

        // DELETE /api/tools/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var entity = await _tools.GetToolByIdAsync(id, ct);
            if (entity is null) return NotFound();

            try
            {
                var ok = await _tools.DeleteToolAsync(id, ct);
                if (!ok) return BadRequest(new { message = "Delete failed" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return NoContent();
        }
    }
}
