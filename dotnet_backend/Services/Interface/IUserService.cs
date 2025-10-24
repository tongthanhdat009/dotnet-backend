using dotnet_backend.Services;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<int> GetTotalUsersAsync();
    Task<UserDto> CreateUserAsync(UserDto userDto);
    Task<UserDto?> UpdateUserAsync(int id, UserDto userDto);
    Task<bool> DeleteUserAsync(int id);
}