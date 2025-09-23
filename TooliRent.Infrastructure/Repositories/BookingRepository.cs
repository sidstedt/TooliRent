using Microsoft.EntityFrameworkCore;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Interfaces;
using TooliRent.Infrastructure.Persistence;

namespace TooliRent.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly TooliRentDbContext _db;
        public BookingRepository(TooliRentDbContext db) { _db = db; }

        public async Task AddAsync(Booking booking, CancellationToken ct)
        {
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<List<Booking>> GetByUserAsync(Guid userId, CancellationToken ct)
        {
            return await _db.Bookings.AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Booking?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
        }

        public async Task<Booking?> GetWithItemsAsync(int id, CancellationToken ct)
        {
            return await _db.Bookings.AsNoTracking()
                .Include(b => b.Items).ThenInclude(i => i.Tool)
                .FirstOrDefaultAsync(b => b.Id == id, ct);
        }

        public async Task<int> GetReservedQuantityAsync(int toolId, DateTime startDate, DateTime endDate, CancellationToken ct)
        {
            var start = startDate.Date; var end = endDate.Date;
            return await _db.BookingItems
                .Where(i => i.ToolId == toolId && i.Booking.StartDate < end && i.Booking.EndDate > start && i.Status == BookingItemStatus.Reserved)
                .SumAsync(i => (int?)i.Quantity) ?? 0;
        }

        public async Task<BookingItem?> GetItemAsync(int bookingItemId, CancellationToken ct)
        {
            return await _db.BookingItems.FirstOrDefaultAsync(i => i.Id == bookingItemId, ct);
        }

        public async Task<int> MarkOverdueAsync(DateTime nowUtc, CancellationToken ct)
        {
            var today = nowUtc.Date;
            var items = await _db.BookingItems.Include(i => i.Booking)
                .Where(i => i.Status == BookingItemStatus.CheckedOut && i.Booking.EndDate < today)
                .ToListAsync(ct);
            foreach (var i in items) i.Status = BookingItemStatus.Overdue;
            await _db.SaveChangesAsync(ct);
            return items.Count;
        }

        public Task UpdateAsync(Booking booking, CancellationToken ct)
        {
            _db.Bookings.Update(booking);
            return _db.SaveChangesAsync(ct);
        }

        public async Task UpdateItemAsync(BookingItem item, CancellationToken ct)
        {
            _db.BookingItems.Update(item);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateStatusAsync(int bookingId, BookingStatus status, CancellationToken ct)
        {
            var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);
            if (b is null) return;
            b.Status = status;
            await _db.SaveChangesAsync(ct);
        }
    }
}
