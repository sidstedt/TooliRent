namespace TooliRent.Domain.ReadModels
{
    public sealed record ToolUsageAggregate(
        int ToolId,
        string ToolName,
        int Quantity
        );
}
