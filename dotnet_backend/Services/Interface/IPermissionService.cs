using System.Collections.Generic;
using System.Threading.Tasks;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;
public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto?> GetPermissionByIdAsync(int id);
    Task<PermissionDto> CreatePermissionAsync(PermissionDto permission);
    Task<PermissionDto?> UpdatePermissionAsync(int PermissionId,PermissionDto permission);
    Task<bool> DeletePermissionAsync(int id);
}