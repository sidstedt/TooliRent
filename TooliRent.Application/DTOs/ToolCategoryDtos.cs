using System;
using System.ComponentModel.DataAnnotations;

namespace TooliRent.Application.DTOs
{
    public sealed class ToolCategoryListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public sealed class ToolCategoryDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class ToolCategoryCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class ToolCategoryUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
