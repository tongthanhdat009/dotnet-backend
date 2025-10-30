using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
                return NotFound(new { message = "Permission not found" });

            return Ok(permission);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PermissionDto permission)
        {
            var newPermission = await _permissionService.CreatePermissionAsync(permission);
            return CreatedAtAction(nameof(GetById), new { id = newPermission.PermissionId }, newPermission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PermissionDto permission)
        {
            if (id != permission.PermissionId)
                return BadRequest(new { message = "Permission ID mismatch" });

            var updatedPermission = await _permissionService.UpdatePermissionAsync(id, permission);
            if (updatedPermission == null)
                return NotFound(new { message = "Permission not found" });

            return Ok(updatedPermission);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            if (!result)
                return NotFound(new { message = "Permission not found" });

            return NoContent();
        }
    }
}
