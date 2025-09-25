using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TooliRent.Application.DTOs;
using TooliRent.Application.Interfaces;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Interfaces;

namespace TooliRent.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IAdminRepository _adminRepo;

        public AdminService(UserManager<ApplicationUser> users, IAdminRepository adminRead)
        {
            _users = users;
            _adminRepo = adminRead;
        }
        public async Task<List<AdminUserListUsersDto>> GetUsersAsync(CancellationToken ct)
        {
            var list = await _users.Users.OrderBy(u => u.Email!).ToListAsync(ct);
            var result = new List<AdminUserListUsersDto>(list.Count);
            foreach (var u in list)
            {
                var roles = await _users.GetRolesAsync(u);
                result.Add(new AdminUserListUsersDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    DisplayName = u.DisplayName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Roles = roles.ToList()
                });
            }
            return result;
        }

        public async Task<AdminUserListUsersDto?> GetUserByEmailAsync(string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new InvalidOperationException("email is required");
            var u = await _users.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct);
            if (u is null) return null;
            var roles = await _users.GetRolesAsync(u);

            return new AdminUserListUsersDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                DisplayName = u.DisplayName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> SetUserActiveAsync(Guid id, bool active, CancellationToken ct)
        {
            var u = await _users.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (u is null) return false;
            u.IsActive = active;
            var res = await _users.UpdateAsync(u);
            return res.Succeeded;
        }
        public async Task<AdminStatsDto> GetStatsAsync(CancellationToken ct)
        {
            var stats = await _adminRepo.GetStatsAsync(ct);
            return new AdminStatsDto
            {
                TotalTools = stats.TotalTools,
                TotalBookings = stats.TotalBookings,
                ActiveBookings = stats.ActiveBookings,
                Members = stats.Members,
                CheckedOutItems = stats.CheckedOutItems,
                OverdueItems = stats.OverdueItems
            };
        }

        public async Task<UsageStatsDto> GetUsageAsync(DateTime from, DateTime to, CancellationToken ct)
        {
            if (from.Date > to.Date) throw new InvalidOperationException("to must be after from");
            var (bookings, itemsCheckedOut, topTools) = await _adminRepo.GetUsageAsync(from, to, ct);

            return new UsageStatsDto
            {
                From = from.Date,
                To = to.Date,
                Bookings = bookings,
                ItemsCheckedOut = itemsCheckedOut,
                TopTools = topTools.Select(tool => new ToolUsageItemDto
                {
                    ToolId = tool.ToolId,
                    ToolName = tool.ToolName,
                    Quantity = tool.Quantity,
                }).ToList()
            };
        }
    }
}
