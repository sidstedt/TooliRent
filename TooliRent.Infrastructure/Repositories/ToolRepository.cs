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
        public ToolRepository(TooliRentDbContext db) { _db = db; }

        public async Task AddAsync(Tool tool, CancellationToken ct)
        {
            _db.Tools.Add(tool);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _db.Tools.FindAsync([id], ct);
            if (entity is null) return false;

            // Prevent delete if tool has booking items
            var hasBookings = await _db.BookingItems.AnyAsync(bi => bi.ToolId == id, ct);
            if (hasBookings)
                throw new InvalidOperationException("Cannot delete a tool that is associated with bookings.");

            _db.Tools.Remove(entity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<Tool>> GetAvailableAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var start = startDate.Date;
            var end = endDate.Date;

            // A tool is available if: Status=Available and QuantityAvailable - reservedInPeriod > 0
            var tools = await _db.Tools
                .Include(t => t.Category)
                .Where(t => t.Status == ToolStatus.Available)
                .ToListAsync(ct);

            var result = new List<Tool>();
            foreach (var t in tools)
            {
                var reserved = await _db.BookingItems
                    .Where(i => i.ToolId == t.Id && i.Booking.StartDate < end && i.Booking.EndDate > start)
                    .SumAsync(i => (int?)i.Quantity, ct) ?? 0;
                if (t.QuantityAvailable - reserved > 0)
                    result.Add(t);
            }
            return result;
        }

        public async Task<Tool?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _db.Tools.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<List<Tool>> SearchAsync(ToolSearchCriteria criteria, CancellationToken ct)
        {
            var q = _db.Tools.AsQueryable();
            q = q.Include(t => t.Category);

            if (!string.IsNullOrWhiteSpace(criteria.Name))
            {
                var name = criteria.Name.Trim();
                q = q.Where(t => t.Name.Contains(name));
            }
            if (criteria.CategoryId.HasValue)
                q = q.Where(t => t.CategoryId == criteria.CategoryId.Value);
            if (criteria.Status.HasValue)
                q = q.Where(t => t.Status == criteria.Status.Value);
            if (criteria.MinPrice.HasValue)
                q = q.Where(t => t.PricePerDay >= criteria.MinPrice.Value);
            if (criteria.MaxPrice.HasValue)
                q = q.Where(t => t.PricePerDay <= criteria.MaxPrice.Value);

            return await q.ToListAsync(ct);
        }

        public async Task<bool> UpdateAsync(Tool tool, CancellationToken ct)
        {
            _db.Tools.Update(tool);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
