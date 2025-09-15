using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;

namespace TooliRent.Domain.Interfaces
{
    public interface IToolRepository
    {
        // Basic CRUD methods
        Task<List<Tool>> GetAllToolsAsync(CancellationToken ct);
        Task<Tool?> GetToolByIdAsync(int id, CancellationToken ct);
        Task AddToolAsync(Tool tool, CancellationToken ct);
        Task UpdateToolAsync(Tool tool, CancellationToken ct);
        Task DeleteToolAsync(int id, CancellationToken ct);

        // Filtered retrieval methods
        Task<List<Tool>> GetToolsByCategoryAsync(int categoryId, CancellationToken ct);
        Task<List<Tool>> SearchAsync(ToolSearchCriteria criteria, CancellationToken ct);

        // Availability check method
        Task<List<Tool>> GetAvailableToolsAsync(DateTime startDate, DateTime endDate, CancellationToken ct);

        // Inventory management methods
        Task<bool> AdjustQuantityAsync(int toolId, int adjustment, CancellationToken ct);
        Task UpdateStatusAsync(int toolId, string status, CancellationToken ct);
    }

    public sealed record ToolSearchCriteria
        (
            string? Name = null,
            int? CategoryId = null,
            ToolStatus? Status = null,
            decimal? MinPrice = null,
            decimal? MaxPrice = null
        );
}
