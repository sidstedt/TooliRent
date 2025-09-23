using TooliRent.Application.DTOs;
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

        // DTO methods
        public async Task<List<BookingListItemDto>> GetUserListAsync(Guid userId, CancellationToken ct)
        {
            var list = await _bookings.GetByUserAsync(userId, ct);
            return list.Select(b => new BookingListItemDto
            {
                Id = b.Id,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Status = (int)b.Status,
                CreatedAt = b.CreatedAt,
                ItemCount = b.Items.Count
            }).ToList();
        }

        public async Task<BookingDetailDto?> GetDetailAsync(int id, Guid userId, CancellationToken ct)
        {
            var booking = await _bookings.GetWithItemsAsync(id, ct);
            if (booking is null || booking.UserId != userId) return null;
            return new BookingDetailDto
            {
                Id = booking.Id,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Status = (int)booking.Status,
                CreatedAt = booking.CreatedAt,
                Items = booking.Items.Select(i => new BookingItemDto
                {
                    ToolId = i.ToolId,
                    ToolName = i.Tool.Name,
                    Quantity = i.Quantity,
                    Status = (int)i.Status
                }).ToList()
            };
        }

        public async Task<int> CreateAsync(Guid userId, CreateBookingDto dto, CancellationToken ct)
        {
            if (dto.StartDate.Date >= dto.EndDate.Date)
                throw new InvalidOperationException("StartDate must be before EndDate.");

            // Validate tools and availability
            var toolIds = dto.Items.Select(i => i.ToolId).Distinct().ToArray();
            var tools = await _tools.SearchAsync(new Domain.Queries.ToolSearchCriteria(), ct);
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
            var booking = new Booking
            {
                UserId = userId,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                Status = BookingStatus.Pending,
                Items = dto.Items.Select(i => new BookingItem
                {
                    ToolId = i.ToolId,
                    Quantity = i.Quantity,
                    Status = BookingItemStatus.Reserved
                }).ToList()
            };
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

            foreach (var item in booking.Items.Where(i => i.Status == BookingItemStatus.CheckedOut || i.Status == BookingItemStatus.Overdue))
            {
                var tool = booking.Items.First(i => i.Id == item.Id).Tool;
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
