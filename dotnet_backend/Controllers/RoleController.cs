using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers
{
    [Authorize] // Bảo vệ toàn bộ controller
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleDto role)
        {
            var newRole = await _roleService.CreateRoleAsync(role);
            return CreatedAtAction(nameof(GetAll), new { id = newRole.RoleId }, newRole);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoleDto role)
        {
            var updatedRole = await _roleService.UpdateRoleAsync(id, role);
            if (updatedRole == null)
            {
                return NotFound();
            }
            return Ok(updatedRole);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
