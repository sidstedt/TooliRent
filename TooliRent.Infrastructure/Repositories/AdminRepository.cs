using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Interfaces;
using TooliRent.Domain.ReadModels;
using TooliRent.Infrastructure.Persistence;

namespace TooliRent.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly TooliRentDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        public AdminRepository(TooliRentDbContext db, UserManager<ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }
        public async Task<AdminStats> GetStatsAsync(CancellationToken ct)
        {
            var totalTools = await _db.Tools.CountAsync(ct);
            var totalBookings = await _db.Bookings.CountAsync(ct);
            var activeBookings = await _db.Bookings
                .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                .CountAsync(ct);
            var members = await _users.Users.CountAsync(ct);
            var checkedOut = await _db.BookingItems
                .Where(i => i.Status == BookingItemStatus.CheckedOut)
                .SumAsync(i => (int?)i.Quantity, ct);
            var overdue = await _db.BookingItems
                .Where(i => i.Status == BookingItemStatus.Overdue)
                .SumAsync(i => (int?)i.Quantity, ct);

            return new AdminStats(
                totalTools,
                totalBookings,
                activeBookings,
                members,
                checkedOut ?? 0,
                overdue ?? 0
            );
        }
        // Check out later... does not work as intended right now...
        public async Task<(int bookings, int itemsCheckedOut, List<ToolUsageAggregate> topTools)> GetUsageAsync(DateTime from, DateTime to, CancellationToken ct)
        {
            var start = from.Date;
            var end = to.Date.AddDays(1);

            var bookings = await _db.Bookings
                .Where(b => b.CreatedAt >= start && b.CreatedAt < end)
                .CountAsync(ct);

            var itemsCheckedOut = await _db.BookingItems
                .Where(i => i.CheckedOutAt >= start && i.CheckedOutAt < end)
                .SumAsync(i => (int?)i.Quantity, ct) ?? 0;

            var topTools = await _db.BookingItems
                .Include(i => i.Tool)
                .Where(i => i.CheckedOutAt >= start && i.CheckedOutAt < end)
                .GroupBy(i => new { i.ToolId, i.Tool!.Name })
                .Select(g => new ToolUsageAggregate(g.Key.ToolId, g.Key.Name, g.Sum(i => i.Quantity)))
                .OrderByDescending(x => x.Quantity)
                .ToListAsync(ct);

            return (bookings, itemsCheckedOut, topTools);
        }
    }
}
