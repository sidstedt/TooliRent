using TooliRent.Domain.Entities;
using TooliRent.Domain.Queries;

namespace TooliRent.Application.Interfaces
{
    public interface IToolService
    {
        // Läs
        Task<List<Tool>> GetAllToolsAsync(CancellationToken ct);
        Task<Tool?> GetToolByIdAsync(int id, CancellationToken ct);
        Task AddToolAsync(Tool tool, CancellationToken ct);

        // Updatera/ta bort
        Task<bool> UpdateToolAsync(Tool tool, CancellationToken ct);
        Task<bool> DeleteToolAsync(int id, CancellationToken ct);

        // Filtrering och sökning
        Task<List<Tool>> GetToolsByCategoryAsync(int categoryId, CancellationToken ct);
        Task<List<Tool>> SearchAsync(ToolSearchCriteria criteria, CancellationToken ct);

        // Tillgänglighet
        Task<List<Tool>> GetAvailableToolsAsync(DateTime startDate, DateTime endDate, CancellationToken ct);

        // Lagerhantering
        Task<bool> AdjustQuantityAsync(int toolId, int adjustment, CancellationToken ct);
        Task UpdateStatusAsync(int toolId, string status, CancellationToken ct);
    }
}
