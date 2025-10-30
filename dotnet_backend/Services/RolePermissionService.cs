using dotnet_backend.Database;
using dotnet_backend.Dtos;
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
}