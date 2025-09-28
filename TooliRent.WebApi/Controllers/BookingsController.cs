using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookings;
        public BookingsController(IBookingService bookings)
        {
            _bookings = bookings;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /api/bookings/mine
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<BookingListItemDto>>> GetMine(CancellationToken ct)
        {
            var userId = GetUserId();
            var items = await _bookings.GetUserListAsync(userId, ct);
            return Ok(items);
        }
        // GET /api/bookings/get-all-admin
        [HttpGet("get-all-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BookingListItemDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var items = await _bookings.GetAllAsync(page, pageSize, ct);
            return Ok(items);
        }

        // GET /api/bookings/{id}/admin
        [HttpGet("{id:int}/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BookingDetailDto>> GetByIdAdmin([FromRoute] int id, CancellationToken ct)
        {
            var dto = await _bookings.GetDetailAsync(id, null, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        // GET /api/bookings/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var userId = GetUserId();
            var dto = await _bookings.GetDetailAsync(id, userId, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        // POST /api/bookings
        [HttpPost]
        [Authorize(Roles = "Member, Admin")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto, CancellationToken ct)
        {
            var userId = GetUserId();
            var id = await _bookings.CreateAsync(userId, dto, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // Cancel /api/bookings/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel([FromRoute] int id, CancellationToken ct)
        {
            var userId = GetUserId();
            var ok = await _bookings.CancelAsync(id, userId, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST /api/bookings/{id}/checkout
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/checkout")]
        public async Task<IActionResult> Checkout([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _bookings.CheckoutAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST /api/bookings/{id}/return
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _bookings.ReturnAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST /api/bookings/overdue/scan
        [Authorize(Roles = "Admin")]
        [HttpPost("overdue/scan")]
        public async Task<IActionResult> ScanOverdue(CancellationToken ct)
        {
            var count = await _bookings.ScanOverdueAsync(DateTime.UtcNow, ct);
            return Ok(new { updated = count });
        }
    }
}
