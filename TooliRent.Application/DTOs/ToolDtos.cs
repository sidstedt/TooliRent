using System.ComponentModel.DataAnnotations;
using TooliRent.Domain.Enums;

namespace TooliRent.Application.DTOs
{
    public sealed class ToolListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public int QuantityAvailable { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public ToolStatus Status { get; set; }
    }

    public sealed class ToolDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }= string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public int QuantityAvailable { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public ToolStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class ToolCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Range(typeof(decimal), "0", "100000.00")]
        public decimal PricePerDay { get; set; }
        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; }
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }

    public sealed class ToolUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Range(typeof(decimal), "0", "100000.00")]
        public decimal PricePerDay { get; set; }
        [Range(0, int.MaxValue)]
        public int QuantityAvailable { get; set; }
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
        public ToolStatus Status { get; set; }
    }
}
