using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly TooliRentDbContext _db;
        private readonly UserManager<ApplicationUser> _users;

        public AdminController(TooliRentDbContext db, UserManager<ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        // POST /api/admin/users/{id}/activate
        [HttpPost("users/{id:guid}/activate")]
        public async Task<IActionResult> ActivateUser([FromRoute] Guid id, CancellationToken ct)
        {
            var user = await _users.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user is null) return NotFound();

            user.IsActive = true;
            await _users.UpdateAsync(user);
            return NoContent();
        }

        // POST /api/admin/users/{id}/deactivate
        [HttpPost("users/{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateUser([FromRoute] Guid id, CancellationToken ct)
        {
            var user = await _users.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (user is null) return NotFound();

            user.IsActive = false;
            await _users.UpdateAsync(user);
            return NoContent();
        }

        // GET /api/admin/stats/overview
        [HttpGet("stats/overview")]
        public async Task<ActionResult<OverviewStatsDto>> GetOverviewStats(CancellationToken ct)
        {
            var totalTools = await _db.Tools.CountAsync(ct);
            var availableTools = await _db.Tools.Where(t => t.Status == ToolStatus.Available && t.QuantityAvailable > 0).CountAsync(ct);
            var checkedOut = await _db.BookingItems.Where(i => i.Status == BookingItemStatus.CheckedOut).SumAsync(i => (int?)i.Quantity) ?? 0;
            var overdue = await _db.BookingItems.Where(i => i.Status == BookingItemStatus.Overdue).SumAsync(i => (int?)i.Quantity) ?? 0;
            var totalBookings = await _db.Bookings.CountAsync(ct);
            var activeBookings = await _db.Bookings.Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed).CountAsync(ct);

            return Ok(new OverviewStatsDto
            {
                TotalTools = totalTools,
                AvailableTools = availableTools,
                CheckedOutItems = checkedOut,
                OverdueItems = overdue,
                TotalBookings = totalBookings,
                ActiveBookings = activeBookings
            });
        }

        // GET /api/admin/stats/usage?from=yyyy-MM-dd&to=yyyy-MM-dd
        [HttpGet("stats/usage")]
        public async Task<ActionResult<UsageStatsDto>> GetUsageStats([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct = default)
        {
            if (from.Date > to.Date) return BadRequest(new { message = "from must be before to" });
            var start = from.Date; var end = to.Date.AddDays(1);

            var bookings = await _db.Bookings.Where(b => b.CreatedAt >= start && b.CreatedAt < end).CountAsync(ct);
            var itemsCheckedOut = await _db.BookingItems.Where(i => i.CheckedOutAt >= start && i.CheckedOutAt < end).SumAsync(i => (int?)i.Quantity) ?? 0;

            var topTools = await _db.BookingItems
                .Include(i => i.Tool)
                .Where(i => i.CheckedOutAt >= start && i.CheckedOutAt < end)
                .GroupBy(i => new { i.ToolId, i.Tool.Name })
                .Select(g => new ToolUsageItemDto
                {
                    ToolId = g.Key.ToolId,
                    ToolName = g.Key.Name,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync(ct);

            return Ok(new UsageStatsDto
            {
                From = start,
                To = to.Date,
                Bookings = bookings,
                ItemsCheckedOut = itemsCheckedOut,
                TopTools = topTools
            });
        }
    }
}
