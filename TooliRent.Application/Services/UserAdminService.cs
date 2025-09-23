using TooliRent.Application.Interfaces;
using TooliRent.Domain.Interfaces;

namespace TooliRent.Application.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly IUserRepository _users;
        public UserAdminService(IUserRepository users) { _users = users; }

        public Task<bool> SetUserStatusAsync(Guid userId, bool isActive, CancellationToken ct)
            => _users.SetStatusAsync(userId, isActive, ct);
    }
}
