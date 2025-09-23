using TooliRent.Application.DTOs;

namespace TooliRent.Application.Interfaces
{
    public interface IToolCategoryService
    {
        // DTO-oriented operations (controller-friendly)
        Task<List<ToolCategoryListItemDto>> GetAllListAsync(CancellationToken ct);
        Task<ToolCategoryDetailDto?> GetDetailAsync(int id, CancellationToken ct);
        Task<ToolCategoryDetailDto> CreateAsync(ToolCategoryCreateDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(int id, ToolCategoryUpdateDto dto, CancellationToken ct);
        Task<bool> DeleteCategoryAsync(int id, CancellationToken ct);
    }
}
