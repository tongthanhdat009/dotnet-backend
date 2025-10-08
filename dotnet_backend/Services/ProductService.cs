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
        // Truy vấn database, join bảng và chuyển đổi sang DTO
        return await _context.Products
            .Include(p => p.Category) // Giả sử bạn có quan hệ với Category
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                CategoryName = p.Category.CategoryName
            }).ToListAsync();
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        //... triển khai logic tương tự
        return null;
    }
}