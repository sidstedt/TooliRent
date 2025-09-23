using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Interfaces;

namespace TooliRent.Application.Services
{
    public class ToolCategoryService : IToolCategoryService
    {
        private readonly IToolCategoryRepository _repo;
        public ToolCategoryService(IToolCategoryRepository repo) { _repo = repo; }

        public Task<bool> AddAsync(ToolCategory category, CancellationToken ct) => _repo.AddAsync(category, ct);
        public Task<bool> DeleteAsync(int id, CancellationToken ct) => _repo.DeleteAsync(id, ct);
        public Task<List<ToolCategory>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);
        public Task<ToolCategory?> GetByIdAsync(int id, CancellationToken ct) => _repo.GetByIdAsync(id, ct);
        public Task<bool> UpdateAsync(ToolCategory category, CancellationToken ct) => _repo.UpdateAsync(category, ct);
    }
}
