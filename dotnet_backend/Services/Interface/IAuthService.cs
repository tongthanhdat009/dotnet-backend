using dotnet_backend.Dtos;
using dotnet_backend.Models;

namespace dotnet_backend.Services;

public interface IAuthService
{
    /// <summary>
    /// Đăng nhập và nhận Access Token + Refresh Token
    /// </summary>
    Task<LoginResponseDto?> LoginAsync(string username, string password);

    /// <summary>
    /// Làm mới Access Token bằng Refresh Token
    /// </summary>
    Task<RefreshResponseDto?> RefreshAccessTokenAsync(string refreshToken);
}
