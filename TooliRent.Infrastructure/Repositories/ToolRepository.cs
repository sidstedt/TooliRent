using Microsoft.EntityFrameworkCore;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Interfaces;
using TooliRent.Domain.Queries;
using TooliRent.Infrastructure.Persistence;

namespace TooliRent.Infrastructure.Repositories
{
    public class ToolRepository : IToolRepository
    {
        private readonly TooliRentDbContext _db;
        public ToolRepository(TooliRentDbContext db) => _db = db;

        public async Task AddToolAsync(Tool tool, CancellationToken ct)
        {
            _db.Tools.Add(tool);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> AdjustQuantityAsync(int toolId, int adjustment, CancellationToken ct)
        {
            var tool = await _db.Tools.FirstOrDefaultAsync(t => t.Id == toolId, ct);
            if (tool is null) return false;
            var newQty = tool.QuantityAvailable + adjustment;
            if (newQty < 0) return false;
            tool.QuantityAvailable = newQty;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task DeleteToolAsync(int id, CancellationToken ct)
        {
            var entity = await _db.Tools.Include(t => t.BookingItems).FirstOrDefaultAsync(t => t.Id == id, ct);
            if (entity is null) return;
            if (entity.BookingItems.Any())
                throw new InvalidOperationException("Cannot delete tool with booking history.");
            _db.Tools.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<List<Tool>> GetAllToolsAsync(CancellationToken ct)
        {
            return await _db.Tools.AsNoTracking().Include(t => t.Category).OrderBy(t => t.Id).ToListAsync(ct);
        }

        public async Task<List<Tool>> GetAvailableToolsAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var start = startDate.Date;
            var end = endDate.Date;

            var reservedInPeriod = _db.BookingItems
                .Where(i => i.Booking!.StartDate < end && i.Booking!.EndDate > start && i.Status != BookingItemStatus.Cancelled && i.Status != BookingItemStatus.Returned)
                .GroupBy(i => i.ToolId)
                .Select(g => new { ToolId = g.Key, Qty = g.Sum(i => i.Quantity) });

            var allocatedNow = _db.BookingItems
                .Where(i => i.Status == BookingItemStatus.Reserved || i.Status == BookingItemStatus.CheckedOut || i.Status == BookingItemStatus.Overdue)
                .GroupBy(i => i.ToolId)
                .Select(g => new { ToolId = g.Key, Qty = g.Sum(i => i.Quantity) });

            var query = from t in _db.Tools.AsNoTracking().Include(t => t.Category)
                        join r in reservedInPeriod on t.Id equals r.ToolId into rj
                        from r in rj.DefaultIfEmpty()
                        join a in allocatedNow on t.Id equals a.ToolId into aj
                        from a in aj.DefaultIfEmpty()
                        let total = t.QuantityAvailable + (a != null ? a.Qty : 0)
                        let reserved = r != null ? r.Qty : 0
                        where total - reserved > 0 && t.Status != ToolStatus.Inactive && t.Status != ToolStatus.Maintenance
                        select t;

            return await query.OrderBy(t => t.Name).ToListAsync(ct);
        }

        public async Task<Tool?> GetToolByIdAsync(int id, CancellationToken ct)
        {
            return await _db.Tools.AsNoTracking().Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<List<Tool>> GetToolsByCategoryAsync(int categoryId, CancellationToken ct)
        {
            return await _db.Tools.AsNoTracking().Include(t => t.Category).Where(t => t.CategoryId == categoryId).ToListAsync(ct);
        }

        public async Task<List<Tool>> SearchAsync(ToolSearchCriteria criteria, CancellationToken ct)
        {
            var q = _db.Tools.AsNoTracking().Include(t => t.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(criteria.Name))
            {
                var s = criteria.Name.Trim();
                q = q.Where(t => t.Name.Contains(s) || t.Description.Contains(s));
            }
            if (criteria.CategoryId.HasValue)
                q = q.Where(t => t.CategoryId == criteria.CategoryId.Value);
            if (criteria.Status.HasValue)
                q = q.Where(t => t.Status == criteria.Status.Value);
            if (criteria.MinPrice.HasValue)
                q = q.Where(t => t.PricePerDay >= criteria.MinPrice.Value);
            if (criteria.MaxPrice.HasValue)
                q = q.Where(t => t.PricePerDay <= criteria.MaxPrice.Value);
            return await q.OrderBy(t => t.Id).ToListAsync(ct);
        }

        public async Task UpdateStatusAsync(int toolId, string status, CancellationToken ct)
        {
            var tool = await _db.Tools.FirstOrDefaultAsync(t => t.Id == toolId, ct);
            if (tool is null) return;
            if (Enum.TryParse<ToolStatus>(status, true, out var parsed))
            {
                tool.Status = parsed;
                await _db.SaveChangesAsync(ct);
            }
            else
            {
                throw new ArgumentException("Invalid status value", nameof(status));
            }
        }

        public async Task UpdateToolAsync(Tool tool, CancellationToken ct)
        {
            _db.Tools.Update(tool);
            await _db.SaveChangesAsync(ct);
        }
    }
}
