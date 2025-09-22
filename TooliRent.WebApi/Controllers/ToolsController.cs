using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TooliRent.Application.DTOs;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Infrastructure.Persistence;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly TooliRentDbContext _db;
        public ToolsController(TooliRentDbContext db) => _db = db;

        // GET /api/tools?categoryId=&status=&availableOnly=&search=&sort=
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolListItemDto>>> Get([FromQuery] int? categoryId, [FromQuery] ToolStatus? status, [FromQuery] bool? availableOnly, [FromQuery] string? search, [FromQuery] string? sort, CancellationToken ct)
        {
            var q = _db.Tools.AsNoTracking().Include(t => t.Category).AsQueryable();

            if (categoryId.HasValue) q = q.Where(t => t.CategoryId == categoryId.Value);
            if (status.HasValue) q = q.Where(t => t.Status == status.Value);
            if (availableOnly == true) q = q.Where(t => t.QuantityAvailable > 0 && t.Status == ToolStatus.Available);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(t => t.Name.Contains(s) || t.Description.Contains(s));
            }

            q = sort?.ToLowerInvariant() switch
            {
                "price_asc" => q.OrderBy(t => t.PricePerDay),
                "price_desc" => q.OrderByDescending(t => t.PricePerDay),
                "name_asc" => q.OrderBy(t => t.Name),
                "name_desc" => q.OrderByDescending(t => t.Name),
                _ => q.OrderBy(t => t.Id)
            };

            var items = await q.Select(t => new ToolListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                PricePerDay = t.PricePerDay,
                QuantityAvailable = t.QuantityAvailable,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name,
                Status = (int)t.Status
            }).ToListAsync(ct);

            return Ok(items);
        }

        // GET /api/tools/{id}
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ToolDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var tool = await _db.Tools.AsNoTracking().Include(t => t.Category)
                .Where(t => t.Id == id)
                .Select(t => new ToolDetailDto
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
                })
                .FirstOrDefaultAsync(ct);

            return tool is null ? NotFound() : Ok(tool);
        }

        // GET /api/tools/available?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD&categoryId=&search=
        [AllowAnonymous]
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<ToolListItemDto>>> GetAvailableInPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? categoryId, [FromQuery] string? search, CancellationToken ct)
        {
            if (startDate.Date >= endDate.Date)
                return BadRequest(new { message = "startDate must be before endDate" });

            var start = startDate.Date;
            var end = endDate.Date;

            var reservedInPeriod = _db.BookingItems
                .Where(i => i.Booking!.StartDate < end && i.Booking!.EndDate > start && i.Status != BookingItemStatus.Cancelled && i.Status != BookingItemStatus.Returned)
                .GroupBy(i => i.ToolId)
                .Select(g => new { ToolId = g.Key, Qty = g.Sum(i => i.Quantity) });

            var allocatedNow = _db.BookingItems
                .Where(i => i.Status == BookingItemStatus.Reserved || i.Status == BookingItemStatus.CheckedOut || i.Status == BookingItemStatus.Overdue)
                .GroupBy(i => i.ToolId)
                .Select(g => new { ToolId = g.Key, Qty = g.Sum(i => i.Quantity) });

            var query = from t in _db.Tools.AsNoTracking().Include(t => t.Category)
                        join r in reservedInPeriod on t.Id equals r.ToolId into rj
                        from r in rj.DefaultIfEmpty()
                        join a in allocatedNow on t.Id equals a.ToolId into aj
                        from a in aj.DefaultIfEmpty()
                        let total = t.QuantityAvailable + (a != null ? a.Qty : 0)
                        let reserved = r != null ? r.Qty : 0
                        where total - reserved > 0 && t.Status != ToolStatus.Inactive && t.Status != ToolStatus.Maintenance
                        select new ToolListItemDto
                        {
                            Id = t.Id,
                            Name = t.Name,
                            PricePerDay = t.PricePerDay,
                            QuantityAvailable = t.QuantityAvailable,
                            CategoryId = t.CategoryId,
                            CategoryName = t.Category.Name,
                            Status = (int)t.Status
                        };

            if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t => t.Name.Contains(s));
            }

            var result = await query.OrderBy(t => t.Name).ToListAsync(ct);
            return Ok(result);
        }

        // POST /api/tools
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ToolCreateDto dto, CancellationToken ct)
        {
            var existsCategory = await _db.ToolCategories.AnyAsync(c => c.Id == dto.CategoryId, ct);
            if (!existsCategory) return BadRequest(new { message = "Category not found." });

            var entity = new Tool
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                PricePerDay = dto.PricePerDay,
                QuantityAvailable = dto.QuantityAvailable,
                CategoryId = dto.CategoryId,
                Status = ToolStatus.Available
            };
            _db.Tools.Add(entity);
            await _db.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new ToolDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                PricePerDay = entity.PricePerDay,
                QuantityAvailable = entity.QuantityAvailable,
                CategoryId = entity.CategoryId,
                CategoryName = (await _db.ToolCategories.FindAsync(new object[] { entity.CategoryId }, ct))?.Name ?? string.Empty,
                Status = (int)entity.Status,
                CreatedAt = entity.CreatedAt
            });
        }

        // PUT /api/tools/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ToolUpdateDto dto, CancellationToken ct)
        {
            var entity = await _db.Tools.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (entity is null) return NotFound();

            var existsCategory = await _db.ToolCategories.AnyAsync(c => c.Id == dto.CategoryId, ct);
            if (!existsCategory) return BadRequest(new { message = "Category not found." });

            if (!Enum.IsDefined(typeof(ToolStatus), dto.Status))
                return BadRequest(new { message = "Invalid status" });

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description.Trim();
            entity.PricePerDay = dto.PricePerDay;
            entity.QuantityAvailable = dto.QuantityAvailable;
            entity.CategoryId = dto.CategoryId;
            entity.Status = (ToolStatus)dto.Status;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // DELETE /api/tools/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var entity = await _db.Tools.Include(t => t.BookingItems).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (entity is null) return NotFound();

            if (entity.BookingItems.Any())
                return BadRequest(new { message = "Cannot delete tool with booking history." });

            _db.Tools.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
