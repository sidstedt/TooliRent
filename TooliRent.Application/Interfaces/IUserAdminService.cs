using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TooliRent.Application.Interfaces
{
    public interface IUserAdminService
    {
        Task<bool> SetUserStatusAsync(Guid userId, bool isActive, CancellationToken ct);
    }
}
