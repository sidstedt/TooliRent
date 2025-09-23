using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TooliRent.Domain.Entities;
using TooliRent.Domain.Interfaces;

namespace TooliRent.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole<Guid>> _roles;

        public UserRepository(UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles)
        {
            _users = users;
            _roles = roles;
        }

        public async Task<bool> AddToRoleAsync(ApplicationUser user, string role, CancellationToken ct)
        {
            if (!await _roles.RoleExistsAsync(role))
            {
                await _roles.CreateAsync(new IdentityRole<Guid>(role));
            }
            var res = await _users.AddToRoleAsync(user, role);
            return res.Succeeded;
        }

        public async Task<bool> CreateAsync(ApplicationUser user, string password, CancellationToken ct)
        {
            var res = await _users.CreateAsync(user, password);
            return res.Succeeded;
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct)
        {
            return await _users.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _users.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct)
        {
            var roles = await _users.GetRolesAsync(user);
            return roles as IReadOnlyCollection<string> ?? roles.ToArray();
        }

        public async Task<bool> SetStatusAsync(Guid userId, bool isActive, CancellationToken ct)
        {
            var user = await GetByIdAsync(userId, ct);
            if (user is null) return false;
            user.IsActive = isActive;
            var res = await _users.UpdateAsync(user);
            return res.Succeeded;
        }
    }
}
