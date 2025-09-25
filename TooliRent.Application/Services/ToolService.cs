using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Interfaces;
using TooliRent.Domain.Queries;

namespace TooliRent.Application.Services
{
    public class ToolService : IToolService
    {
        private readonly IToolRepository _repo;
        private readonly IToolCategoryRepository _categories;
        public ToolService(IToolRepository repo, IToolCategoryRepository categories)
        {
            _repo = repo;
            _categories = categories;
        }

        // DTO-oriented operations
        public async Task<List<ToolListItemDto>> SearchListAsync(ToolSearchCriteria criteria, bool availableOnly, CancellationToken ct)
        {
            var list = await _repo.SearchAsync(criteria, ct);
            if (availableOnly)
                list = list.Where(t => t.QuantityAvailable > 0 && t.Status == ToolStatus.Available).ToList();

            return list
                .OrderBy(t => t.Id)
                .Select(ToListItem)
                .ToList();
        }

        public async Task<ToolDetailDto?> GetDetailAsync(int id, CancellationToken ct)
        {
            var t = await _repo.GetByIdAsync(id, ct);
            if (t is null) return null;
            return ToDetail(t);
        }

        public async Task<List<ToolListItemDto>> GetAvailableListAsync(DateTime startDate, DateTime endDate, int? categoryId, CancellationToken ct)
        {
            var list = await _repo.GetAvailableAsync(startDate, endDate, ct);
            if (categoryId.HasValue)
                list = list.Where(t => t.CategoryId == categoryId.Value).ToList();

            return list
                .OrderBy(t => t.Name)
                .Select(ToListItem)
                .ToList();
        }

        public async Task<ToolDetailDto> CreateAsync(ToolCreateDto dto, CancellationToken ct)
        {
            var category = await _categories.GetByIdAsync(dto.CategoryId, ct);
            if (category is null) throw new InvalidOperationException("Category not found.");

            var entity = new Tool
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                PricePerDay = dto.PricePerDay,
                QuantityAvailable = dto.QuantityAvailable,
                CategoryId = dto.CategoryId,
                Status = ToolStatus.Available
            };
            await _repo.AddAsync(entity, ct);

            var created = await _repo.GetByIdAsync(entity.Id, ct);
            return ToDetail(created ?? entity);
        }

        public async Task<bool> UpdateAsync(int id, ToolUpdateDto dto, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            var category = await _categories.GetByIdAsync(dto.CategoryId, ct);
            if (category is null) throw new InvalidOperationException("Category not found.");

            if (!Enum.IsDefined(typeof(ToolStatus), dto.Status))
                throw new InvalidOperationException("Invalid status");

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description.Trim();
            entity.PricePerDay = dto.PricePerDay;
            entity.QuantityAvailable = dto.QuantityAvailable;
            entity.CategoryId = dto.CategoryId;
            entity.Status = (ToolStatus)dto.Status;

            return await _repo.UpdateAsync(entity, ct);
        }

        public Task<bool> DeleteAsync(int id, CancellationToken ct) => _repo.DeleteAsync(id, ct);

        private static ToolListItemDto ToListItem(Tool t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            PricePerDay = t.PricePerDay,
            QuantityAvailable = t.QuantityAvailable,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? string.Empty,
            Status = (int)t.Status
        };

        private static ToolDetailDto ToDetail(Tool t) => new()
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            PricePerDay = t.PricePerDay,
            QuantityAvailable = t.QuantityAvailable,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? string.Empty,
            Status = (int)t.Status,
            CreatedAt = t.CreatedAt
        };
    }
}
