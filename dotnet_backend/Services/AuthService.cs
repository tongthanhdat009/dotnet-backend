using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using dotnet_backend.Database;
using dotnet_backend.Dtos;
using dotnet_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Xác thực người dùng và trả về Access Token + Refresh Token
    /// </summary>
    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        // Tìm user theo username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user == null)
            return null;

        // Kiểm tra password
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            return null;

        // Lấy danh sách permissions của user dựa trên roleId
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == user.Role)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.ActionKey)
            .ToListAsync();

        // Tạo tokens
        var accessToken = await GenerateAccessToken(user, permissions);
        var refreshToken = GenerateRefreshToken(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Permissions = permissions
        };
    }

    /// <summary>
    /// Refresh Access Token bằng Refresh Token (không cần lưu RT trong DB)
    /// </summary>
    public async Task<RefreshResponseDto?> RefreshAccessTokenAsync(string refreshToken)
    {
        // Vì chúng ta không lưu RT trong DB, chúng ta chỉ kiểm tra format
        // và giả sử frontend sẽ gửi đúng RT (có thể validate JWT signature nếu cần)

        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        try
        {
            // ✅ Optional: Validate RT JWT signature để đảm bảo nó không bị giả mạo
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "");

            tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return null;

            // Lấy user từ DB
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return null;

            // Lấy lại permissions
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == user.Role)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission.ActionKey)
                .ToListAsync();

            // Tạo Access Token mới
            var newAccessToken = await GenerateAccessToken(user, permissions);

            return new RefreshResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = refreshToken // Trả lại RT cũ (hoặc có thể tạo RT mới nếu muốn)
            };
        }
        catch
        {
            // RT không hợp lệ hoặc hết hạn
            return null;
        }
    }

    /// <summary>
    /// Tạo Access Token (thời gian sống ngắn: 15 phút)
    /// Thêm danh sách permissions vào claims
    /// </summary>
    private async Task<string> GenerateAccessToken(User user, List<string> permissions)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "");

        var claims = new List<Claim>
        {
            new Claim("sub", user.UserId.ToString()), // Subject = UserId
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? ""),
            new Claim(ClaimTypes.GivenName, user.FullName ?? ""),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // Thêm permissions vào claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60), // AT hết hạn sau 60 phút
            Issuer = _configuration["Jwt:Issuer"] ?? "",
            Audience = _configuration["Jwt:Audience"] ?? "",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Tạo Refresh Token (thời gian sống dài: 7 ngày)
    /// Token này sẽ được mã hóa JWT để client có thể kiểm tra hạn sử dụng
    /// </summary>
    private string GenerateRefreshToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "");

        // RT cần chứa userId để biết refresh cho user nào
        var claims = new List<Claim>
        {
            new Claim("sub", user.UserId.ToString()), // userId để validate
            new Claim("type", "refresh")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), // RT hết hạn sau 7 ngày
            Issuer = _configuration["Jwt:Issuer"] ?? "",
            Audience = _configuration["Jwt:Audience"] ?? "",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
