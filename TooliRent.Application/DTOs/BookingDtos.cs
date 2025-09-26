using System.ComponentModel.DataAnnotations;
using TooliRent.Domain.Enums;

namespace TooliRent.Application.DTOs
{
    public sealed class BookingItemCreateDto
    {
        [Range(1,int.MaxValue)]
        public int ToolId { get; set; }
        [Range(1,int.MaxValue)]
        public int Quantity { get; set; }
    }

    public sealed class CreateBookingDto
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public List<BookingItemCreateDto> Items { get; set; } = new();
    }

    public sealed class BookingListItemDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
    }

    public sealed class BookingItemDto
    {
        public int ToolId { get; set; }
        public string ToolName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public BookingItemStatus Status { get; set; }
    }

    public sealed class BookingDetailDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BookingItemDto> Items { get; set; } = new();
    }
}
