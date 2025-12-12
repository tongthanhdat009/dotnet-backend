namespace dotnet_backend.Dtos;

public class ValidateCartStockResponse
{
    public bool IsValid { get; set; }
    public List<OutOfStockProduct> OutOfStockProducts { get; set; } = new();
}

public class OutOfStockProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}
