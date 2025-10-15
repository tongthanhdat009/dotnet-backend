using dotnet_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using dotnet_backend.Database;
using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Implement the methods defined in IUserService here
    // Get all users
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Password = u.Password,
                FullName = u.FullName,
                Role = u.Role
            })
            .ToListAsync();
    }

    // Get user by ID
    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var userDto = await _context.Users
            .Where(u => u.UserId == id)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Password = u.Password,
                FullName = u.FullName,
                Role = u.Role
            })
            .FirstOrDefaultAsync();

        return userDto;
    }

    // Create a new user
    public async Task<UserDto> CreateUserAsync(UserDto userDto)
    {
        var user = new User
        {
            Username = userDto.Username,
            Password = userDto.Password,
            FullName = userDto.FullName,
            Role = userDto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        userDto.UserId = user.UserId; // Assign the generated ID back to the DTO
        return userDto;
    }

    // Update an existing user
    public async Task<UserDto?> UpdateUserAsync(int id, UserDto userDto)
    {
        var userToUpdate = await _context.Users.FindAsync(id);
        if (userToUpdate == null)
        {
            return null;
        }
        // update fields
        userToUpdate.Username = userDto.Username;
        userToUpdate.Password = userDto.Password;
        userToUpdate.FullName = userDto.FullName;
        userToUpdate.Role = userDto.Role;

        // save changes to database
        await _context.SaveChangesAsync();

        // return updated user DTO
        return new UserDto
        {
            UserId = userToUpdate.UserId,
            Username = userToUpdate.Username,
            Password = userToUpdate.Password,
            FullName = userToUpdate.FullName,
            Role = userToUpdate.Role
        };

    }
    
    // Delete a user
    public async Task<bool> DeleteUserAsync(int id)
    {
        var userToDelete = await _context.Users.FindAsync(id);
        if (userToDelete == null)
        {
            return false; // User not found
        }

        _context.Users.Remove(userToDelete);
        await _context.SaveChangesAsync();
        return true; // User successfully deleted
    }
}