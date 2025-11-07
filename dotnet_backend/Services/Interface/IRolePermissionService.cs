using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IRolePermissionService
{
    Task<IEnumerable<RolePermissionDto>> GetAllRolePermissionsAsync();
    Task<RolePermissionDto> GetRolePermissionByIdAsync(int id);
    Task<RolePermissionDto> CreateRolePermissionAsync(RolePermissionDto rolePermissionDto);
    Task<bool> UpdateRolePermissionAsync(RolePermissionDto rolePermissionDto);
    Task<bool> DeleteRolePermissionAsync(int id);
    Task AssignPermissionToRoleAsync(int roleId, int permissionId);
    Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
    Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(int roleId);
}