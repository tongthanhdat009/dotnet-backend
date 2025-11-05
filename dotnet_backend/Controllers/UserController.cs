using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users - Ai đã login cũng xem được
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/5 - Ai đã login cũng xem được
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user is null) return NotFound();
            return Ok(user);
        }

        // POST: api/users - CHỈ Admin hoặc Manager mới tạo được user
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserDto userDto)
        {
            // fixed validation
            UserDto createdUser = null;
            try
            {
                createdUser = await _userService.CreateUserAsync(userDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.UserId }, createdUser);

        }


        // PUT: api/users/5 - CHỈ Admin hoặc Manager mới update được
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.UserId)
            {
                return BadRequest(new { message = "ID trên endpoint khác với body" });
            }

            UserDto updatedUser = null;
            try
            {
                updatedUser = await _userService.UpdateUserAsync(id, userDto);
                if (updatedUser is null) return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return Ok(updatedUser);
        }

        // DELETE: api/users/5 - CHỈ Admin mới xóa được
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deleted = await _userService.DeleteUserAsync(id);
            if (!deleted) return NotFound(new { message = "Không thể xóa tài khoản" });
            return Ok(new { message = "Xóa tài khoản thành công" });
        }
    }
}
