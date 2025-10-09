namespace dotnet_backend.Dtos;

public class UserDto
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public int? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderDto> Orders { get; set; } = new List<OrderDto>();

    public virtual RoleDto? RoleNavigation { get; set; }
}