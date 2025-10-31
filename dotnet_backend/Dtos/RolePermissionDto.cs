namespace dotnet_backend.Dtos;

public class RolePermissionDto
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    // Thông tin chi tiết của Role (optional)
    public string? RoleName { get; set; }
    public string? RoleDescription { get; set; }

    // Thông tin chi tiết của Permission (optional)
    public string? PermissionName { get; set; }
    public string? ActionKey { get; set; }
    public string? PermissionDescription { get; set; }
}

public class RolePermissionCreateDto
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}

public class RolePermissionResponseDto
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string ActionKey { get; set; } = string.Empty;
}