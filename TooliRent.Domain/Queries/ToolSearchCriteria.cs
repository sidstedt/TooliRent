using TooliRent.Domain.Enums;

namespace TooliRent.Domain.Queries
{
    public sealed record ToolSearchCriteria(
        string? Name = null,
        int? CategoryId = null,
        ToolStatus? Status = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null
    );
}
