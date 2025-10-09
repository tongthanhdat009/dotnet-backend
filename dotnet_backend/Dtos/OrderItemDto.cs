namespace dotnet_backend.Dtos;

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int? Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
    public virtual OrderDto? Order { get; set; }
    public virtual ProductDto? Product { get; set; }
}