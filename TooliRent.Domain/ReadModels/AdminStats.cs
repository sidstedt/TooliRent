namespace TooliRent.Domain.ReadModels
{
    public sealed record AdminStats
    (
        int TotalTools,
        int TotalBookings,
        int ActiveBookings,
        int Members,
        int? CheckedOutItems = null,
        int? OverdueItems = null
    );
}
