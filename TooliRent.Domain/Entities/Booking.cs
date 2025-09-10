using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TooliRent.Domain.Enums;

namespace TooliRent.Domain.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
    }
}
