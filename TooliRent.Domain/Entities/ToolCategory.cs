using System.ComponentModel.DataAnnotations;

namespace TooliRent.Domain.Entities;

public class ToolCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;


    public ICollection<Tool> Tools { get; set; } = new List<Tool>();
}