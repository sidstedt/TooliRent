using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;

namespace TooliRent.Application.Interfaces
{
    public interface IBookingService
    {
        // Läs
        Task<Booking?> GetByIdAsync(int id, CancellationToken ct);
        Task<Booking?> GetWithItemsAsync(int id, CancellationToken ct);
        Task<List<Booking>> GetByUserAsync(Guid userId, CancellationToken ct);

        // Skapa/uppdatera
        Task AddAsync(Booking booking, CancellationToken ct);
        Task UpdateAsync(Booking booking, CancellationToken ct);

        // Avboka / status
        Task UpdateStatusAsync(int bookingId, BookingStatus status, CancellationToken ct);

        // Tillgänglighet
        Task<int> GetReservedQuantityAsync(int toolId, DateTime startDate, DateTime endDate, CancellationToken ct);

        // Utlämning / återlämning
        Task<BookingItem?> GetItemAsync(int bookingItemId, CancellationToken ct);
        Task UpdateItemAsync(BookingItem item, CancellationToken ct);

        // Försenade
        Task<int> MarkOverdueAsync(DateTime nowUtc, CancellationToken ct);
    }
}
