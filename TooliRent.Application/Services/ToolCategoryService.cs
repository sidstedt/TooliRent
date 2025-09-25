using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Interfaces;
using AutoMapper;

namespace TooliRent.Application.Services
{
    public class ToolCategoryService : IToolCategoryService
    {
        private readonly IToolCategoryRepository _repo;
        private readonly IMapper _mapper;
        public ToolCategoryService(IToolCategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // DTO-oriented
        public async Task<List<ToolCategoryListItemDto>> GetAllListAsync(CancellationToken ct)
        {
            var cat = await _repo.GetAllAsync(ct);
            //return cat.OrderBy(c => c.Name).Select(c => new ToolCategoryListItemDto
            //{
            //    Id = c.Id,
            //    Name = c.Name,
            //    IsActive = c.IsActive
            //}).ToList();
            return _mapper.Map<List<ToolCategoryListItemDto>>(cat.OrderBy(c => c.Name));
        }

        public async Task<ToolCategoryDetailDto?> GetDetailAsync(int id, CancellationToken ct)
        {
            var c = await _repo.GetByIdAsync(id, ct);
            return c is null ? null : _mapper.Map<ToolCategoryDetailDto>(c);
        }

        public async Task<ToolCategoryDetailDto> CreateAsync(ToolCategoryCreateDto dto, CancellationToken ct)
        {
            var existing = await _repo.GetAllAsync(ct);
            if (existing.Any(c => string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Category name must be unique.");

            var entity = _mapper.Map<ToolCategory>(dto);
            await _repo.AddAsync(entity, ct);

            return _mapper.Map<ToolCategoryDetailDto>(entity);
        }

        public async Task<bool> UpdateAsync(int id, ToolCategoryUpdateDto dto, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            var cats = await _repo.GetAllAsync(ct);
            if (cats.Any(c => c.Id != id && string.Equals(c.Name, dto.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Category name must be unique.");

            _mapper.Map(dto, entity);
            return await _repo.UpdateAsync(entity, ct);
        }

        public async Task<bool> DeleteCategoryAsync(int id, CancellationToken ct)
        {
            return await _repo.DeleteAsync(id, ct);
        }
    }
}
