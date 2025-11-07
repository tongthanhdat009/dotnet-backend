using dotnet_backend.Database;
using dotnet_backend.Dtos;
using dotnet_backend.Models;
using dotnet_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly ApplicationDbContext _context;

    public RolePermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermissionDto>> GetAllRolePermissionsAsync()
    {
        var rolePermissions = await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .ToListAsync();

        return rolePermissions.Select(rp => new RolePermissionDto
        {
            RoleId = rp.RoleId,
            PermissionId = rp.PermissionId,
            RoleName = rp.Role?.RoleName,
            RoleDescription = rp.Role?.Description,
            PermissionName = rp.Permission?.PermissionName,
            ActionKey = rp.Permission?.ActionKey,
            PermissionDescription = rp.Permission?.Description
        });
    }
    public async Task<RolePermissionDto> GetRolePermissionByIdAsync(int id)
    {
        var rp = await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == id);

        if (rp == null) return null;

        return new RolePermissionDto
        {
            RoleId = rp.RoleId,
            PermissionId = rp.PermissionId,
            RoleName = rp.Role?.RoleName,
            RoleDescription = rp.Role?.Description,
            PermissionName = rp.Permission?.PermissionName,
            ActionKey = rp.Permission?.ActionKey,
            PermissionDescription = rp.Permission?.Description
        };
    }

    public async Task<RolePermissionDto> CreateRolePermissionAsync(RolePermissionDto rolePermissionDto)
    {
        var rolePermission = new RolePermission
        {
            RoleId = rolePermissionDto.RoleId,
            PermissionId = rolePermissionDto.PermissionId
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        return rolePermissionDto;
    }

    public async Task<bool> UpdateRolePermissionAsync(RolePermissionDto rolePermissionDto)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == rolePermissionDto.RoleId && rp.PermissionId == rolePermissionDto.PermissionId);

        if (rolePermission == null)
            return false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRolePermissionAsync(int id)
    {
        var rolePermission = await _context.RolePermissions.FindAsync(id);
        if (rolePermission == null)
            return false;

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (!exists)
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(int roleId)
    {
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => new PermissionDto
            {
                PermissionId = rp.Permission.PermissionId,
                PermissionName = rp.Permission.PermissionName,
                ActionKey = rp.Permission.ActionKey,
                Description = rp.Permission.Description
            })
            .ToListAsync();

        return permissions;
    }
}