using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;
        public AdminController(IAdminService admin)
        {
            _admin = admin;
        }

        // GET /api/admin/users
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<AdminUserListUsersDto>>> GetUsers(CancellationToken ct)
        {
            var result = await _admin.GetUsersAsync(ct);
            return Ok(result);
        }

        // GET /api/admin/users/by-email?email=...
        [HttpGet("users/by-email")]
        public async Task<ActionResult<AdminUserListUsersDto>> GetUserByEmail([FromQuery] string email, CancellationToken ct)
        {
            var res = await _admin.GetUserByEmailAsync(email, ct);
            if (res is null) return NotFound();
            return Ok(res);
        }

        // POST /api/admin/users/{id}/activate
        [HttpPost("users/{id:guid}/activate")]
        public async Task<IActionResult> ActivateUser([FromRoute] Guid id, CancellationToken ct)
        {
            var ok = await _admin.SetUserActiveAsync(id, true, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST /api/admin/users/{id}/deactivate
        [HttpPost("users/{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateUser([FromRoute] Guid id, CancellationToken ct)
        {
            var ok = await _admin.SetUserActiveAsync(id, false, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // GET /api/admin/stats
        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats(CancellationToken ct)
        {
            var dto = await _admin.GetStatsAsync(ct);
            return Ok(dto);
        }

        // GET /api/admin/stats/usage?from=yyyy-MM-dd&to=yyyy-MM-dd
        [HttpGet("stats/usage")]
        public async Task<ActionResult<UsageStatsDto>> GetUsageStats([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct = default)
        {
            var dto = await _admin.GetUsageAsync(from, to, ct);
            return Ok(dto);
        }
    }
}
