public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Barcode { get; set; }
    public string Unit { get; set; }

    public int? CategoryId { get; set; }

    public int? SupplierId { get; set; }
}
