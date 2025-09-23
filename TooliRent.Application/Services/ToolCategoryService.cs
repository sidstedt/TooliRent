using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Interfaces;

namespace TooliRent.Application.Services
{
    public class ToolCategoryService : IToolCategoryService
    {
        private readonly IToolCategoryRepository _repo;
        public ToolCategoryService(IToolCategoryRepository repo)
        {
            _repo = repo;
        }

        // DTO-oriented
        public async Task<List<ToolCategoryListItemDto>> GetAllListAsync(CancellationToken ct)
        {
            var cats = await _repo.GetAllAsync(ct);
            return cats.OrderBy(c => c.Name).Select(c => new ToolCategoryListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<ToolCategoryDetailDto?> GetDetailAsync(int id, CancellationToken ct)
        {
            var c = await _repo.GetByIdAsync(id, ct);
            if (c is null) return null;
            return new ToolCategoryDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            };
        }

        public async Task<ToolCategoryDetailDto> CreateAsync(ToolCategoryCreateDto dto, CancellationToken ct)
        {
            var existing = await _repo.GetAllAsync(ct);
            if (existing.Any(c => string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Category name must be unique.");

            var entity = new ToolCategory
            {
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim(),
                IsActive = dto.IsActive
            };
            await _repo.AddAsync(entity, ct);
            return new ToolCategoryDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, ToolCategoryUpdateDto dto, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            var cats = await _repo.GetAllAsync(ct);
            if (cats.Any(c => c.Id != id && string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Category name must be unique.");

            entity.Name = dto.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim();
            entity.IsActive = dto.IsActive;
            return await _repo.UpdateAsync(entity, ct);
        }

        public async Task<bool> DeleteCategoryAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;
            if (entity.Tools.Any())
                throw new InvalidOperationException("Cannot delete category with tools.");
            return await _repo.DeleteAsync(id, ct);
        }
    }
}
