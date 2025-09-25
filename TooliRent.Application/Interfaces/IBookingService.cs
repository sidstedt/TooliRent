using TooliRent.Application.DTOs;

namespace TooliRent.Application.Interfaces
{
    public interface IBookingService
    {
        // DTO-orienterade operationer (controller-vänliga)
        Task<List<BookingListItemDto>> GetUserListAsync(Guid userId, CancellationToken ct);
        Task<BookingDetailDto?> GetDetailAsync(int id, Guid userId, CancellationToken ct);
        Task<int> CreateAsync(Guid userId, CreateBookingDto dto, CancellationToken ct);
        Task<bool> CancelAsync(int id, Guid userId, CancellationToken ct);
        Task<bool> CheckoutAsync(int id, CancellationToken ct);
        Task<bool> ReturnAsync(int id, CancellationToken ct);
        Task<int> ScanOverdueAsync(DateTime nowUtc, CancellationToken ct);
    }
}
