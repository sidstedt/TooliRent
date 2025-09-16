using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TooliRent.Domain.Entities;

namespace TooliRent.Application.Interfaces
{
    public interface IToolCategoryService
    {
        // CRUD
        Task<bool> AddAsync(ToolCategory category, CancellationToken ct);
        Task<List<ToolCategory>> GetAllAsync(CancellationToken ct);
        Task<bool> UpdateAsync(ToolCategory category, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
