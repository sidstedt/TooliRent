using AutoMapper;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Enums;
using TooliRent.Domain.Interfaces;
using TooliRent.Domain.Queries;

namespace TooliRent.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookings;
        private readonly IToolRepository _tools;
        private readonly IMapper _mapper;
        public BookingService(IBookingRepository bookings, IToolRepository tools, IMapper mapper)
        {
            _bookings = bookings;
            _tools = tools;
            _mapper = mapper;
        }

        public async Task<List<BookingListItemDto>> GetUserListAsync(Guid userId, CancellationToken ct)
        {
            var list = await _bookings.GetByUserAsync(userId, ct);
            return _mapper.Map<List<BookingListItemDto>>(list ?? new List<Booking>());
        }

        public async Task<List<BookingListItemDto>> GetAllAsync(int page, int pageSize, CancellationToken ct)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;
            var allBookings = await _bookings.GetAllAsync(ct);
            var paged = allBookings
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<BookingListItemDto>>(paged);
        }

        public async Task<BookingDetailDto?> GetDetailAsync(int id, Guid? userId, CancellationToken ct)
        {
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null || (userId.HasValue && booking.UserId != userId.Value)) return null;
            return _mapper.Map<BookingDetailDto>(booking);
        }

        public async Task<int> CreateAsync(Guid userId, CreateBookingDto dto, CancellationToken ct)
        {
            if (dto.StartDate.Date >= dto.EndDate.Date)
                throw new InvalidOperationException("StartDate must be before EndDate.");

            // Validate tools and availability
            var toolIds = dto.Items.Select(i => i.ToolId).Distinct().ToArray();
            var tools = await _tools.SearchAsync(new ToolSearchCriteria(), ct);
            tools = tools.Where(t => toolIds.Contains(t.Id)).ToList();
            if (tools.Count != toolIds.Length)
                throw new InvalidOperationException("One or more tools not found.");

            foreach (var line in dto.Items)
            {
                var tool = tools.First(t => t.Id == line.ToolId);
                if (tool.Status != ToolStatus.Available || tool.QuantityAvailable < line.Quantity)
                    throw new InvalidOperationException($"Tool {tool.Name} not available in requested quantity.");
            }

            // Create booking and items
            var booking = _mapper.Map<Booking>(dto);
            booking.UserId = userId;

            await _bookings.AddAsync(booking, ct);

            // Decrease available quantities
            foreach (var line in dto.Items)
            {
                var tool = tools.First(t => t.Id == line.ToolId);
                tool.QuantityAvailable -= line.Quantity;
                await _tools.UpdateAsync(tool, ct);
            }

            return booking.Id;
        }

        public async Task<bool> CancelAsync(int id, Guid userId, CancellationToken ct)
        {
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null || booking.UserId != userId) return false;

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new InvalidOperationException("Booking cannot be cancelled.");

            var anyProcessed = booking.Items.Any(i =>
                i.Status == BookingItemStatus.CheckedOut ||
                i.Status == BookingItemStatus.Returned ||
                i.Status == BookingItemStatus.Overdue);

            if (booking.Status == BookingStatus.Confirmed || anyProcessed)
                throw new InvalidOperationException("Booking cannot be cancelled after checkout.");

            // Restore quantities only for reserved items not yet checked out
            foreach (var item in booking.Items.Where(i => i.Status == BookingItemStatus.Reserved))
            {
                item.Status = BookingItemStatus.Cancelled;
                var tool = booking.Items.First(i => i.Id == item.Id).Tool;
                if (tool != null)
                {
                    tool.QuantityAvailable += item.Quantity;
                    await _tools.UpdateAsync(tool, ct);
                }
            }
            booking.Status = BookingStatus.Cancelled;
            await _bookings.UpdateAsync(booking, ct);
            return true;
        }

        public async Task<bool> CheckoutAsync(int id, CancellationToken ct)
        {
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null) return false;
            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new InvalidOperationException("Booking not in a check-outable state.");

            foreach (var item in booking.Items.Where(i => i.Status == BookingItemStatus.Reserved))
            {
                item.Status = BookingItemStatus.CheckedOut;
                item.CheckedOutAt = DateTime.UtcNow;
            }
            booking.Status = BookingStatus.Confirmed;
            await _bookings.UpdateAsync(booking, ct);
            return true;
        }

        public async Task<bool> ReturnAsync(int id, CancellationToken ct)
        {
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null) return false;

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return false;

            var toReturn = booking.Items
                .Where(i => i.Status == BookingItemStatus.CheckedOut ||
                i.Status == BookingItemStatus.Overdue).ToList();

            if (toReturn.Count == 0)
                throw new InvalidOperationException("booking cannot be returned before checkout.");

            foreach (var item in toReturn)
            {
                var tool = item.Tool;
                if (tool != null)
                {
                    tool.QuantityAvailable += item.Quantity;
                    await _tools.UpdateAsync(tool, ct);
                }
                item.Status = BookingItemStatus.Returned;
                item.ReturnedAt = DateTime.UtcNow;
            }

            if (booking.Items.All(i => i.Status == BookingItemStatus.Returned || i.Status == BookingItemStatus.Cancelled))
            {
                booking.Status = BookingStatus.Completed;
            }

            await _bookings.UpdateAsync(booking, ct);
            return true;
        }

        public Task<int> ScanOverdueAsync(DateTime nowUtc, CancellationToken ct) => _bookings.MarkOverdueAsync(nowUtc, ct);
    }
}
