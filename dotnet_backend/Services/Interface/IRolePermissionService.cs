using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IRolePermissionService
{
    Task<IEnumerable<RolePermissionDto>> GetAllRolePermissionsAsync();
}