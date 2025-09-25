using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TooliRent.Application.DTOs
{
    public class AdminStatsDto
    {
        public int TotalTools { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int Members { get; set; }
        public int? CheckedOutItems { get; set; }
        public int? OverdueItems { get; set; }
    }

    public sealed class ToolUsageItemDto
    {
        public int ToolId { get; set; }
        public string ToolName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public sealed class UsageStatsDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Bookings { get; set; }
        public int ItemsCheckedOut { get; set; }
        public List<ToolUsageItemDto> TopTools { get; set; } = new();
    }
}
