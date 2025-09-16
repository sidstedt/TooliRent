using TooliRent.Domain.Entities;

namespace TooliRent.Domain.Interfaces
{
    public interface IToolCategoryRepository
    {
        // Läs
        Task<ToolCategory?> GetByIdAsync(int id, CancellationToken ct);
        Task<List<ToolCategory>> GetAllAsync(CancellationToken ct);

        // Skapa / Uppdatera / Ta bort
        Task<bool> AddAsync(ToolCategory category, CancellationToken ct);
        Task<bool> UpdateAsync(ToolCategory category, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
