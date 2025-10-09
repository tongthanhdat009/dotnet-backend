namespace dotnet_backend.Dtos;

public class PermissionDto
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;

    public string ActionKey { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<RoleDto> Roles { get; set; } = new List<RoleDto>();
}
