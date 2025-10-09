// Services/ProductService.cs
using Microsoft.EntityFrameworkCore;
using dotnet_backend.Database;
using dotnet_backend.Services.Interface;

namespace dotnet_backend.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    // Dùng Dependency Injection để inject DbContext vào
    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
{
    return await _context.Products
    .Include(p => p.Category)
    .Include(p => p.Supplier)
    .Select(p => new ProductDto
    {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        Price = p.Price,
        Barcode = p.Barcode,
        Unit = p.Unit,
        CategoryId = p.CategoryId,
        CategoryName = p.Category != null ? p.Category.CategoryName : "-",
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier != null ? p.Supplier.Name : "-"
    }).ToListAsync();

}

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        //... triển khai logic tương tự
        return null;
    }
}