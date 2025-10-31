using Microsoft.EntityFrameworkCore;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;
using dotnet_backend.Models;
using dotnet_backend.Database;

namespace dotnet_backend.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    ActionKey = p.ActionKey,
                    Description = p.Description
                })
                .ToListAsync();
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
        {
            return await _context.Permissions
                .Where(p => p.PermissionId == id)
                .Select(p => new PermissionDto
                {
                    PermissionId = p.PermissionId,
                    PermissionName = p.PermissionName,
                    ActionKey = p.ActionKey,
                    Description = p.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PermissionDto> CreatePermissionAsync(PermissionDto permission)
        {
            var newPermission = new Permission
            {
                PermissionName = permission.PermissionName,
                ActionKey = permission.ActionKey,
                Description = permission.Description
            };

            _context.Permissions.Add(newPermission);
            await _context.SaveChangesAsync();

            return new PermissionDto
            {
                PermissionId = newPermission.PermissionId,
                PermissionName = newPermission.PermissionName,
                ActionKey = newPermission.ActionKey,
                Description = newPermission.Description
            };
        }

        public async Task<PermissionDto?> UpdatePermissionAsync(int permissionId, PermissionDto updatedPermission)
        {
            var existing = await _context.Permissions.FindAsync(permissionId);
            if (existing == null) return null;

            existing.ActionKey = updatedPermission.ActionKey;
            existing.PermissionName = updatedPermission.PermissionName;
            existing.Description = updatedPermission.Description;

            await _context.SaveChangesAsync();

            return new PermissionDto
            {
                PermissionId = existing.PermissionId,
                PermissionName = existing.PermissionName,
                ActionKey = existing.ActionKey,
                Description = existing.Description
            };
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return false;

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
