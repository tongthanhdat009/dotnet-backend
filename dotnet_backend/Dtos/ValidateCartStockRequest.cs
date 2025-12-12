namespace dotnet_backend.Dtos;

public class ValidateCartStockRequest
{
    public List<CartItemStockRequest> Items { get; set; } = new();
}

public class CartItemStockRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
