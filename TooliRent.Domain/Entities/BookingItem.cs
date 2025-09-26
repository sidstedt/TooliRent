using System.ComponentModel.DataAnnotations;
using TooliRent.Domain.Enums;

namespace TooliRent.Domain.Entities
{
    public class BookingItem
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int ToolId { get; set; }
        public Tool Tool { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public BookingItemStatus Status { get; set; } = BookingItemStatus.Reserved;

        public DateTime? CheckedOutAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}
