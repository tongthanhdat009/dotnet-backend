using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Endpoint đăng nhập: nhận username + password, trả về Access Token + Refresh Token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username và Password là bắt buộc" });
        }

        // Xác thực user
        var result = await _authService.LoginAsync(request.Username, request.Password);

        if (result == null)
        {
            return Unauthorized(new { message = "Username hoặc Password không chính xác" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Endpoint làm mới Access Token: nhận Refresh Token, trả về Access Token mới
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshResponseDto>> Refresh([FromBody] RefreshRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh Token là bắt buộc" });
        }

        var result = await _authService.RefreshAccessTokenAsync(request.RefreshToken);

        if (result == null)
        {
            return Unauthorized(new { message = "Refresh Token không hợp lệ hoặc đã hết hạn" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Test endpoint: kiểm tra xem AccessToken có hợp lệ không
    /// (Yêu cầu gửi kèm Authorization header với Bearer token)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var fullName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            userId,
            username,
            fullName,
            role,
            message = "AccessToken hợp lệ!"
        });
    }
}
