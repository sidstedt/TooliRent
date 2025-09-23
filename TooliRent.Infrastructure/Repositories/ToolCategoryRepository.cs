using Microsoft.EntityFrameworkCore;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Interfaces;
using TooliRent.Infrastructure.Persistence;

namespace TooliRent.Infrastructure.Repositories
{
    public class ToolCategoryRepository : IToolCategoryRepository
    {
        private readonly TooliRentDbContext _db;
        public ToolCategoryRepository(TooliRentDbContext db) => _db = db;

        public async Task<bool> AddAsync(ToolCategory category, CancellationToken ct)
        {
            _db.ToolCategories.Add(category);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _db.ToolCategories.Include(c => c.Tools).FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity is null) return false;
            if (entity.Tools.Any()) return false;
            _db.ToolCategories.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<ToolCategory>> GetAllAsync(CancellationToken ct)
        {
            return await _db.ToolCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
        }

        public async Task<ToolCategory?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _db.ToolCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<bool> UpdateAsync(ToolCategory category, CancellationToken ct)
        {
            _db.ToolCategories.Update(category);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
