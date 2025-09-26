using System.ComponentModel.DataAnnotations;
using TooliRent.Domain.Enums;

namespace TooliRent.Domain.Entities
{
    public class Tool
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "100000.00")]
        public decimal PricePerDay { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public ToolCategory Category { get; set; } = null!;

        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();

        public ToolStatus Status { get; set; } = ToolStatus.Available;
    }

}