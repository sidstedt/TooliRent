using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Queries;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookings;
        private readonly IToolService _tools;
        public BookingsController(IBookingService bookings, IToolService tools)
        {
            _bookings = bookings;
            _tools = tools;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /api/bookings (own bookings)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingListItemDto>>> GetMine(CancellationToken ct)
        {
            var userId = GetUserId();
            var list = await _bookings.GetByUserAsync(userId, ct);
            var items = list.Select(b => new BookingListItemDto
            {
                Id = b.Id,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Status = (int)b.Status,
                CreatedAt = b.CreatedAt,
                ItemCount = b.Items.Count
            }).ToList();
            return Ok(items);
        }

        // GET /api/bookings/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var userId = GetUserId();
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null || booking.UserId != userId) return NotFound();

            var dto = new BookingDetailDto
            {
                Id = booking.Id,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Status = (int)booking.Status,
                CreatedAt = booking.CreatedAt,
                Items = booking.Items.Select(i => new BookingItemDto
                {
                    ToolId = i.ToolId,
                    ToolName = i.Tool.Name,
                    Quantity = i.Quantity,
                    Status = (int)i.Status
                }).ToList()
            };
            return Ok(dto);
        }

        // POST /api/bookings
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto, CancellationToken ct)
        {
            if (dto.StartDate.Date >= dto.EndDate.Date)
                return BadRequest(new { message = "StartDate must be before EndDate." });

            var userId = GetUserId();

            // Validate tools and availability
            var toolIds = dto.Items.Select(i => i.ToolId).Distinct().ToArray();
            var tools = await _tools.SearchAsync(new ToolSearchCriteria(), ct);
            tools = tools.Where(t => toolIds.Contains(t.Id)).ToList();
            if (tools.Count != toolIds.Length)
                return BadRequest(new { message = "One or more tools not found." });

            foreach (var line in dto.Items)
            {
                var tool = tools.First(t => t.Id == line.ToolId);
                if (tool.Status != ToolStatus.Available || tool.QuantityAvailable < line.Quantity)
                    return BadRequest(new { message = $"Tool {tool.Name} not available in requested quantity." });
            }

            // Create booking and items
            var booking = new Booking
            {
                UserId = userId,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                Status = BookingStatus.Pending,
                Items = dto.Items.Select(i => new BookingItem
                {
                    ToolId = i.ToolId,
                    Quantity = i.Quantity,
                    Status = BookingItemStatus.Reserved
                }).ToList()
            };
            await _bookings.AddAsync(booking, ct);

            // Decrease available quantities
            foreach (var line in dto.Items)
            {
                var tool = tools.First(t => t.Id == line.ToolId);
                tool.QuantityAvailable -= line.Quantity;
                await _tools.UpdateToolAsync(tool, ct);
            }

            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, new { booking.Id });
        }

        // DELETE /api/bookings/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel([FromRoute] int id, CancellationToken ct)
        {
            var userId = GetUserId();
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null || booking.UserId != userId) return NotFound();
            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                return BadRequest(new { message = "Booking cannot be cancelled." });

            // Restore quantities only for reserved items not yet checked out
            foreach (var item in booking.Items.Where(i => i.Status == BookingItemStatus.Reserved))
            {
                item.Status = BookingItemStatus.Cancelled;
                var tool = booking.Items.First(i => i.Id == item.Id).Tool;
                if (tool != null)
                {
                    tool.QuantityAvailable += item.Quantity;
                    await _tools.UpdateToolAsync(tool, ct);
                }
            }
            booking.Status = BookingStatus.Cancelled;
            await _bookings.UpdateAsync(booking, ct);
            return NoContent();
        }

        // POST /api/bookings/{id}/checkout
        [Authorize(Roles="Admin")]
        [HttpPost("{id:int}/checkout")]
        public async Task<IActionResult> Checkout([FromRoute] int id, CancellationToken ct)
        {
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null) return NotFound();
            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                return BadRequest(new { message = "Booking not in a check-outable state." });

            foreach (var item in booking.Items.Where(i => i.Status == BookingItemStatus.Reserved))
            {
                item.Status = BookingItemStatus.CheckedOut;
                item.CheckedOutAt = DateTime.UtcNow;
            }
            booking.Status = BookingStatus.Confirmed;
            await _bookings.UpdateAsync(booking, ct);
            return NoContent();
        }

        // POST /api/bookings/{id}/return
        [Authorize(Roles="Admin")]
        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return([FromRoute] int id, CancellationToken ct)
        {
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null) return NotFound();

            foreach (var item in booking.Items.Where(i => i.Status == BookingItemStatus.CheckedOut || i.Status == BookingItemStatus.Overdue))
            {
                var tool = booking.Items.First(i => i.Id == item.Id).Tool;
                if (tool != null)
                {
                    tool.QuantityAvailable += item.Quantity;
                    await _tools.UpdateToolAsync(tool, ct);
                }
                item.Status = BookingItemStatus.Returned;
                item.ReturnedAt = DateTime.UtcNow;
            }

            if (booking.Items.All(i => i.Status == BookingItemStatus.Returned || i.Status == BookingItemStatus.Cancelled))
            {
                booking.Status = BookingStatus.Completed;
            }

            await _bookings.UpdateAsync(booking, ct);
            return NoContent();
        }

        // POST /api/bookings/overdue/scan
        [Authorize(Roles = "Admin")]
        [HttpPost("overdue/scan")]
        public async Task<IActionResult> ScanOverdue(CancellationToken ct)
        {
            var count = await _bookings.MarkOverdueAsync(DateTime.UtcNow, ct);
            return Ok(new { updated = count });
        }
    }
}
