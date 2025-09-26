using TooliRent.Application.DTOs;
using TooliRent.Domain.Queries;

namespace TooliRent.Application.Interfaces
{
    public interface IToolService
    {
        // DTO-oriented operations (controller-friendly, encapsulate validation and mapping)
        Task<List<ToolListItemDto>> SearchListAsync(ToolSearchCriteria criteria, bool availableOnly, CancellationToken ct);
        Task<ToolDetailDto?> GetDetailAsync(int id, CancellationToken ct);
        Task<List<ToolListItemDto>> GetAvailableListAsync(DateTime startDate, DateTime endDate, int? categoryId, CancellationToken ct);
        Task<ToolDetailDto> CreateAsync(ToolCreateDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(int id, ToolUpdateDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
