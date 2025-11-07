// Services/ProductService.cs
using Microsoft.EntityFrameworkCore;
using dotnet_backend.Database;
using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private static int? _cachedProductCount = null;
    private static DateTime? _cacheTime = null;

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
            SupplierId = p.SupplierId,
            Category = p.Category != null ? new CategoryDto
            {
                CategoryId = p.Category.CategoryId,
                CategoryName = p.Category.CategoryName
            } : null,
            Supplier = p.Supplier != null ? new SupplierDto
            {
                SupplierId = p.Supplier.SupplierId,
                Name = p.Supplier.Name,
                Phone = p.Supplier.Phone,
                Email = p.Supplier.Email,
                Address = p.Supplier.Address
            } : null
        }).ToListAsync();

    }

    public async Task<IEnumerable<TopProductDto>> GetTopProductsByOrderCountAsync(int topCount = 3)
    {
        var result = await _context.OrderItems
            .Join(
                _context.Products,
                oi => oi.ProductId,        
                p => p.ProductId,          
                (oi, p) => new { p.ProductName, oi.ProductId }
            )
            .GroupBy(x => x.ProductName)
            .Select(g => new TopProductDto
            {
                ProductName = g.Key,
                TotalOrders = g.Count()
            })
            .OrderByDescending(x => x.TotalOrders)
            .Take(topCount)
            .ToListAsync();

        return result;
    }

    public async Task<int> GetTotalProductsAsync()
    {
        // Cache kết quả trong 5 phút để tránh query chậm
        if (_cachedProductCount.HasValue && _cacheTime.HasValue && 
            (DateTime.Now - _cacheTime.Value).TotalMinutes < 5)
        {
            return _cachedProductCount.Value;
        }

        var count = await _context.Products.CountAsync();
        _cachedProductCount = count;
        _cacheTime = DateTime.Now;
        return count;
    }

    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        var productDto = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == id)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Barcode = p.Barcode,
                Unit = p.Unit,
                CategoryId = p.CategoryId,
                SupplierId = p.SupplierId,
                Category = p.Category != null ? new CategoryDto
                {
                    CategoryId = p.Category.CategoryId,
                    CategoryName = p.Category.CategoryName
                } : null,
                Supplier = p.Supplier != null ? new SupplierDto
                {
                    SupplierId = p.Supplier.SupplierId,
                    Name = p.Supplier.Name,
                    Phone = p.Supplier.Phone,
                    Email = p.Supplier.Email,
                    Address = p.Supplier.Address
                } : null
            })
            .FirstOrDefaultAsync();

        return productDto;
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
    {
        // 1. Kiểm tra CategoryId có tồn tại không?
        if (productDto.CategoryId.HasValue && !await _context.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId.Value))
        {
            throw new ArgumentException($"Đã tồn tại danh mục với Id {productDto.CategoryId}.");
        }

        // 2. Kiểm tra SupplierId có tồn tại không?
        if (productDto.SupplierId.HasValue && !await _context.Suppliers.AnyAsync(s => s.SupplierId == productDto.SupplierId.Value))
        {
            throw new ArgumentException($"Đã tồn tại nhà cung cấp với Id {productDto.SupplierId}.");
        }

        // 3. Kiểm tra Barcode đã tồn tại chưa? (nếu có cung cấp)
        if (!string.IsNullOrEmpty(productDto.Barcode) && await _context.Products.AnyAsync(p => p.Barcode == productDto.Barcode))
        {
            throw new InvalidOperationException($"Đã tồn tại sản phẩm với mã vạch '{productDto.Barcode}'.");
        }

        // 4. Kiểm tra ProductName đã tồn tại chưa?
        if (await _context.Products.AnyAsync(p => p.ProductName == productDto.ProductName))
        {
            throw new InvalidOperationException($"Đã tồn tại sản phẩm với tên '{productDto.ProductName}'.");
        }

        // 5. Kiểm tra giá hợp lệ
        if (productDto.Price <= 0)
        {
            throw new ArgumentException("Giá phải lớn hơn 0.");
        }

        var product = new Product
        {
            ProductName = productDto.ProductName,
            Price = productDto.Price,
            Barcode = productDto.Barcode,
            Unit = productDto.Unit,
            CategoryId = productDto.CategoryId,
            SupplierId = productDto.SupplierId
        };

        var inventory = new Inventory
        {
            Product = product,
            Quantity = 0 // Luôn khởi tạo tồn kho là 0
        };

        _context.Products.Add(product);
        _context.Inventories.Add(inventory);

        // Lưu tất cả thay đổi trong một giao dịch duy nhất
        await _context.SaveChangesAsync();

        // Lấy lại thông tin product với Category và Supplier để trả về đầy đủ
        var createdProduct = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == product.ProductId)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Barcode = p.Barcode,
                Unit = p.Unit,
                CategoryId = p.CategoryId,
                SupplierId = p.SupplierId,
                Category = p.Category != null ? new CategoryDto
                {
                    CategoryId = p.Category.CategoryId,
                    CategoryName = p.Category.CategoryName
                } : null,
                Supplier = p.Supplier != null ? new SupplierDto
                {
                    SupplierId = p.Supplier.SupplierId,
                    Name = p.Supplier.Name,
                    Phone = p.Supplier.Phone,
                    Email = p.Supplier.Email,
                    Address = p.Supplier.Address
                } : null
            })
            .FirstOrDefaultAsync();

        return createdProduct;
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, ProductDto productDto)
    {
        var productToUpdate = await _context.Products.FindAsync(id);
        if (productToUpdate == null)
        {
            return null;
        }

        // 1. Kiểm tra CategoryId có tồn tại không?
        if (productDto.CategoryId.HasValue && !await _context.Categories.AnyAsync(c => c.CategoryId == productDto.CategoryId.Value))
        {
            throw new ArgumentException($"Đã tồn tại danh mục với Id {productDto.CategoryId}.");
        }

        // 2. Kiểm tra SupplierId có tồn tại không?
        if (productDto.SupplierId.HasValue && !await _context.Suppliers.AnyAsync(s => s.SupplierId == productDto.SupplierId.Value))
        {
            throw new ArgumentException($"Đã tồn tại nhà cung cấp với Id {productDto.SupplierId}.");
        }

        // 3. Kiểm tra Barcode mới có bị trùng với một sản phẩm KHÁC không?
        if (!string.IsNullOrEmpty(productDto.Barcode) &&
         await _context.Products.AnyAsync(p => p.Barcode == productDto.Barcode && p.ProductId != productToUpdate.ProductId))
        {
            throw new InvalidOperationException($"Đã tồn tại sản phẩm với mã vạch '{productDto.Barcode}'.");
        }

        // 4. Kiểm tra ProductName mới có bị trùng với một sản phẩm KHÁC không?
        if (await _context.Products.AnyAsync(p => p.ProductName == productDto.ProductName && p.ProductId != id))
        {
            throw new InvalidOperationException($"Đã tồn tại sản phẩm với tên '{productDto.ProductName}'.");
        }

        // 5. Kiểm tra giá hợp lệ
        if (productDto.Price <= 0)
        {
            throw new ArgumentException("Giá phải lớn hơn 0.");
        }

        // Cập nhật các thuộc tính của entity đã được Entity Framework theo dõi
        productToUpdate.ProductName = productDto.ProductName;
        productToUpdate.Price = productDto.Price;
        productToUpdate.Barcode = productDto.Barcode;
        productToUpdate.Unit = productDto.Unit;
        productToUpdate.CategoryId = productDto.CategoryId;
        productToUpdate.SupplierId = productDto.SupplierId;

        // Lưu các thay đổi vào database
        await _context.SaveChangesAsync();

        // Tương tự như hàm Create, ta truy vấn lại để lấy đầy đủ thông tin CategoryName và SupplierName
        var updatedProduct = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == id)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Barcode = p.Barcode,
                Unit = p.Unit,
                CategoryId = p.CategoryId,
                SupplierId = p.SupplierId,
                Category = p.Category != null ? new CategoryDto
                {
                    CategoryId = p.Category.CategoryId,
                    CategoryName = p.Category.CategoryName
                } : null,
                Supplier = p.Supplier != null ? new SupplierDto
                {
                    SupplierId = p.Supplier.SupplierId,
                    Name = p.Supplier.Name,
                    Phone = p.Supplier.Phone,
                    Email = p.Supplier.Email,
                    Address = p.Supplier.Address
                } : null
            })
            .FirstOrDefaultAsync();

        return updatedProduct;
    }
    public async Task<bool> DeleteProductAsync(int id)
    {
        var productToDelete = await _context.Products.FindAsync(id);
        if (productToDelete == null)
        {
            return false;
        }

        // Xóa sản phẩm
        _context.Products.Remove(productToDelete);

        // Lưu các thay đổi vào database
        await _context.SaveChangesAsync();
        return true;
    }
}