using System.Collections.Generic;
using System.Threading.Tasks;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDto> GetRoleByIdAsync(int id);
    Task<RoleDto> CreateRoleAsync(RoleDto role);
    Task<RoleDto> UpdateRoleAsync(int RoleId,RoleDto role);
    Task<bool> DeleteRoleAsync(int id);
}