using TooliRent.Domain.Entities;
using TooliRent.Domain.Queries;

namespace TooliRent.Domain.Interfaces
{
    public interface IToolRepository
    {
        Task<List<Tool>> SearchAsync(ToolSearchCriteria criteria, CancellationToken ct);
        Task<Tool?> GetByIdAsync(int id, CancellationToken ct);
        Task<List<Tool>> GetAvailableAsync(DateTime startDate, DateTime endDate, CancellationToken ct);
        Task AddAsync(Tool tool, CancellationToken ct);
        Task<bool> UpdateAsync(Tool tool, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
