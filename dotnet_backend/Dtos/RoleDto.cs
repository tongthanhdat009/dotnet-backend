namespace dotnet_backend.Dtos;

public class RoleDto
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<UserDto> Users { get; set; } = new List<UserDto>();

    public virtual ICollection<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}