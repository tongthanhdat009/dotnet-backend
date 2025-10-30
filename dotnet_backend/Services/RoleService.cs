using Microsoft.EntityFrameworkCore;
using dotnet_backend.Dtos;
using dotnet_backend.Models;
using dotnet_backend.Services.Interface;
using dotnet_backend.Database;

namespace dotnet_backend.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;

        public RoleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Select(role => new RoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description
                })
                .ToListAsync();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .Where(role => role.RoleId == id)
                .Select(role => new RoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<RoleDto> CreateRoleAsync(RoleDto role)
        {
            var newRole = new Role
            {
                RoleName = role.RoleName,
                Description = role.Description 
            };
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            return new RoleDto
            {
                RoleId = newRole.RoleId,
                RoleName = newRole.RoleName,
                Description = newRole.Description
            };
        }

        public async Task<RoleDto?> UpdateRoleAsync(int id, RoleDto updatedRole)
        {
            var existingRole = await _context.Roles.FindAsync(id);
            if (existingRole == null) return null;

            existingRole.RoleName = updatedRole.RoleName;
            existingRole.Description = updatedRole.Description;
            await _context.SaveChangesAsync();

            return new RoleDto
            {
                RoleId = existingRole.RoleId,
                RoleName = existingRole.RoleName,
                Description = existingRole.Description
            };
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
