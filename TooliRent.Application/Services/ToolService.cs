using AutoMapper;
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
        private readonly IMapper _mapper;
        public ToolService(IToolRepository repo, IToolCategoryRepository categories, IMapper mapper)
        {
            _repo = repo;
            _categories = categories;
            _mapper = mapper;
        }

        public async Task<List<ToolListItemDto>> SearchListAsync(ToolSearchCriteria criteria, bool availableOnly, CancellationToken ct)
        {
            var list = await _repo.SearchAsync(criteria, ct);
            if (availableOnly)
                list = list.Where(t => t.QuantityAvailable > 0 && t.Status == ToolStatus.Available).ToList();

            var ordered = list.OrderBy(t => t.Name).ToList();
            return _mapper.Map<List<ToolListItemDto>>(ordered);
        }

        public async Task<ToolDetailDto?> GetDetailAsync(int id, CancellationToken ct)
        {
            var t = await _repo.GetByIdAsync(id, ct);
            return t is null ? null : _mapper.Map<ToolDetailDto>(t);
        }

        public async Task<List<ToolListItemDto>> GetAvailableListAsync(DateTime startDate, DateTime endDate, int? categoryId, CancellationToken ct)
        {
            var list = await _repo.GetAvailableAsync(startDate, endDate, ct);
            if (categoryId.HasValue)
                list = list.Where(t => t.CategoryId == categoryId.Value).ToList();

            return _mapper.Map<List<ToolListItemDto>>(list.OrderBy(t => t.Name).ToList());
        }

        public async Task<ToolDetailDto> CreateAsync(ToolCreateDto dto, CancellationToken ct)
        {
            var category = await _categories.GetByIdAsync(dto.CategoryId, ct);
            if (category is null) throw new InvalidOperationException("Category not found.");

            var entity = _mapper.Map<Tool>(dto);
            await _repo.AddAsync(entity, ct);

            var created = await _repo.GetByIdAsync(entity.Id, ct) ?? entity;
            return _mapper.Map<ToolDetailDto>(created);
        }

        public async Task<bool> UpdateAsync(int id, ToolUpdateDto dto, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            var category = await _categories.GetByIdAsync(dto.CategoryId, ct);
            if (category is null) throw new InvalidOperationException("Category not found.");

            if (!Enum.IsDefined(typeof(ToolStatus), dto.Status))
                throw new InvalidOperationException("Invalid status");

            _mapper.Map(dto, entity);

            return await _repo.UpdateAsync(entity, ct);
        }

        public Task<bool> DeleteAsync(int id, CancellationToken ct) => _repo.DeleteAsync(id, ct);
    }
}
