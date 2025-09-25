using TooliRent.Application.DTOs;

namespace TooliRent.Application.Interfaces
{
    public interface IAdminService
    {
        Task<List<AdminUserListUsersDto>> GetUsersAsync(CancellationToken ct);
        Task<AdminUserListUsersDto?> GetUserByEmailAsync(string email, CancellationToken ct);
        Task<bool> SetUserActiveAsync(Guid id, bool active, CancellationToken ct);
        Task<AdminStatsDto> GetStatsAsync(CancellationToken ct);
        Task<UsageStatsDto> GetUsageAsync(DateTime from, DateTime to, CancellationToken ct);
    }
}
