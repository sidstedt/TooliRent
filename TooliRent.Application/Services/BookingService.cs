using System.Security.Claims;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Interfaces;

namespace TooliRent.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookings;
        private readonly IToolRepository _tools;
        public BookingService(IBookingRepository bookings, IToolRepository tools)
        {
            _bookings = bookings;
            _tools = tools;
        }

        public Task AddAsync(Booking booking, CancellationToken ct) => _bookings.AddAsync(booking, ct);

        public Task<List<Booking>> GetByUserAsync(Guid userId, CancellationToken ct) => _bookings.GetByUserAsync(userId, ct);

        public Task<Booking?> GetByIdAsync(int id, CancellationToken ct) => _bookings.GetByIdAsync(id, ct);

        public Task<Booking?> GetWithItemsAsync(int id, CancellationToken ct) => _bookings.GetWithItemsAsync(id, ct);

        public Task<int> GetReservedQuantityAsync(int toolId, DateTime startDate, DateTime endDate, CancellationToken ct) => _bookings.GetReservedQuantityAsync(toolId, startDate, endDate, ct);

        public Task<BookingItem?> GetItemAsync(int bookingItemId, CancellationToken ct) => _bookings.GetItemAsync(bookingItemId, ct);

        public Task<int> MarkOverdueAsync(DateTime nowUtc, CancellationToken ct) => _bookings.MarkOverdueAsync(nowUtc, ct);

        public Task UpdateAsync(Booking booking, CancellationToken ct) => _bookings.UpdateAsync(booking, ct);

        public Task UpdateItemAsync(BookingItem item, CancellationToken ct) => _bookings.UpdateItemAsync(item, ct);

        public Task UpdateStatusAsync(int bookingId, BookingStatus status, CancellationToken ct) => _bookings.UpdateStatusAsync(bookingId, status, ct);
    }
}
