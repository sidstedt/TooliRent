using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TooliRent.Domain.Entities;

namespace TooliRent.Application.Interfaces
{
    public interface IUserService
    {
        // Läs
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct);

        // Skapa
        Task<bool> CreateAsync(ApplicationUser user, string password, CancellationToken ct);

        // Roller
        Task<bool> AddToRoleAsync(ApplicationUser user, string role, CancellationToken ct);
        Task<IReadOnlyCollection<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct);
    }
}
