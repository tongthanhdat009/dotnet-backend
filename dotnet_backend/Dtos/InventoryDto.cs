namespace dotnet_backend.Dtos;

public class InventoryDto
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int? Quantity { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual ProductDto Product { get; set; } = null!;
}