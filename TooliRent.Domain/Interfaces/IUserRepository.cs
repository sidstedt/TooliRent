using TooliRent.Domain.Entities;

namespace TooliRent.Domain.Interfaces
{
    public interface IUserRepository
    {
        // Läs
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct);

        // Skapa
        Task<bool> CreateAsync(ApplicationUser user, string password, CancellationToken ct);

        // Roller
        Task<bool> AddToRoleAsync(ApplicationUser user, string role, CancellationToken ct);
        Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct);

        // Admin funktioner
        Task<bool> SetStatusAsync(Guid userId, bool isActive, CancellationToken ct);
    }
}
