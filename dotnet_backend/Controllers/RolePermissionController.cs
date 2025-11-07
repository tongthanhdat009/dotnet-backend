using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers;

[Authorize] // Bảo vệ toàn bộ controller
[ApiController]
[Route("api/[controller]")]
public class RolePermissionController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    public RolePermissionController(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RolePermissionDto>>> GetAllRolePermissions()
    {
        try
        {
            var rolePermissions = await _rolePermissionService.GetAllRolePermissionsAsync();
            return Ok(rolePermissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignPermissionToRole([FromBody] RolePermissionDto dto)
    {
        try
        {
            await _rolePermissionService.AssignPermissionToRoleAsync(dto.RoleId, dto.PermissionId);
            return Ok(new { message = "Gán quyền thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gán quyền", error = ex.Message });
        }
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemovePermissionFromRole([FromBody] RolePermissionDto dto)
    {
        try
        {
            await _rolePermissionService.RemovePermissionFromRoleAsync(dto.RoleId, dto.PermissionId);
            return Ok(new { message = "Xóa quyền thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xóa quyền", error = ex.Message });
        }
    }

    [HttpGet("role/{roleId}")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissionsByRole(int roleId)
    {
        try
        {
            var permissions = await _rolePermissionService.GetPermissionsByRoleIdAsync(roleId);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy permissions", error = ex.Message });
        }
    }
}