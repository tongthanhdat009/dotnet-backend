namespace dotnet_backend.Dtos;

public class SupplierDto
{
    public int SupplierId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<ProductDto> Products { get; set; } = new List<ProductDto>();
}