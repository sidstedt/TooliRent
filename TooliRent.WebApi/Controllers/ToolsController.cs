using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Queries;

namespace TooliRent.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly IToolService _tools;
        public ToolsController(IToolService tools)
        {
            _tools = tools;
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

            var items = await _tools.SearchListAsync(criteria, availableOnly == true, ct);
            return Ok(items);
        }

        // GET /api/tools/{id}
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ToolDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var dto = await _tools.GetDetailAsync(id, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        // GET /api/tools/available?startDate=YYYY-MM-DD&endDate=YYYY-MM-DD&categoryId
        [AllowAnonymous]
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<ToolListItemDto>>> GetAvailableInPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? categoryId, CancellationToken ct)
        {
            if (startDate.Date >= endDate.Date)
                return BadRequest(new { message = "startDate must be before endDate" });

            var result = await _tools.GetAvailableListAsync(startDate, endDate, categoryId, ct);
            return Ok(result);
        }

        // POST /api/tools
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ToolCreateDto dto, CancellationToken ct)
        {
            var created = await _tools.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/tools/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ToolUpdateDto dto, CancellationToken ct)
        {
            var ok = await _tools.UpdateAsync(id, dto, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE /api/tools/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _tools.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
