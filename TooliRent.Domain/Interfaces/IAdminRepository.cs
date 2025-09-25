using TooliRent.Domain.ReadModels;

namespace TooliRent.Domain.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminStats> GetStatsAsync(CancellationToken ct);
        Task<(int bookings, int itemsCheckedOut, List<ToolUsageAggregate> topTools)> GetUsageAsync(DateTime from, DateTime to, CancellationToken ct);
    }
}
