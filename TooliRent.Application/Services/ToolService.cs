using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Interfaces;
using TooliRent.Domain.Queries;

namespace TooliRent.Application.Services
{
    public class ToolService : IToolService
    {
        private readonly IToolRepository _repo;
        public ToolService(IToolRepository repo) { _repo = repo; }

        public Task AddToolAsync(Tool tool, CancellationToken ct) => _repo.AddToolAsync(tool, ct);
        public Task<bool> AdjustQuantityAsync(int toolId, int adjustment, CancellationToken ct) => _repo.AdjustQuantityAsync(toolId, adjustment, ct);
        public Task<bool> DeleteToolAsync(int id, CancellationToken ct) => ExecuteSafe(async () => { await _repo.DeleteToolAsync(id, ct); return true; }, false);
        public Task<List<Tool>> GetAllToolsAsync(CancellationToken ct) => _repo.GetAllToolsAsync(ct);
        public Task<List<Tool>> GetAvailableToolsAsync(DateTime startDate, DateTime endDate, CancellationToken ct) => _repo.GetAvailableToolsAsync(startDate, endDate, ct);
        public Task<Tool?> GetToolByIdAsync(int id, CancellationToken ct) => _repo.GetToolByIdAsync(id, ct);
        public Task<List<Tool>> GetToolsByCategoryAsync(int categoryId, CancellationToken ct) => _repo.GetToolsByCategoryAsync(categoryId, ct);
        public Task<List<Tool>> SearchAsync(ToolSearchCriteria criteria, CancellationToken ct) => _repo.SearchAsync(criteria, ct);
        public Task UpdateStatusAsync(int toolId, string status, CancellationToken ct) => _repo.UpdateStatusAsync(toolId, status, ct);
        public Task<bool> UpdateToolAsync(Tool tool, CancellationToken ct) => ExecuteSafe(async () => { await _repo.UpdateToolAsync(tool, ct); return true; }, false);

        private static async Task<T> ExecuteSafe<T>(Func<Task<T>> action, T fallback)
        {
            try { return await action(); }
            catch { return fallback; }
        }
    }
}
