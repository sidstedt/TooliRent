using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;

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
            var items = await _service.GetAllListAsync(ct);
            return Ok(items);
        }

        // GET /api/toolcategories/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ToolCategoryDetailDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var dto = await _service.GetDetailAsync(id, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        // POST /api/toolcategories
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ToolCategoryCreateDto dto, CancellationToken ct)
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT /api/toolcategories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ToolCategoryUpdateDto dto, CancellationToken ct)
        {
            var ok = await _service.UpdateAsync(id, dto, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE /api/toolcategories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _service.DeleteCategoryAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
