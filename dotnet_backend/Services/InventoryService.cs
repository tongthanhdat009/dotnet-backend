using dotnet_backend.Services.Interface;
using dotnet_backend.Database;
using dotnet_backend.Models;
using dotnet_backend.Dtos;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
    {
        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .ThenInclude(p => p.Category)
            .Include(i => i.Product)
            .ThenInclude(p => p.Supplier)
            .ToListAsync();

        return inventories.Select(i => new InventoryDto
        {
            InventoryId = i.InventoryId,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UpdatedAt = i.UpdatedAt,
            Product = new ProductDto
            {
                ProductId = i.Product.ProductId,
                CategoryId = i.Product.CategoryId,
                SupplierId = i.Product.SupplierId,
                ProductName = i.Product.ProductName,
                Barcode = i.Product.Barcode,
                Price = i.Product.Price,
                Unit = i.Product.Unit,
                CreatedAt = i.Product.CreatedAt
            }
        });
    }

    public async Task<InventoryDto?> GetInventoryByIdAsync(int inventoryId)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .ThenInclude(p => p.Category)
            .Include(i => i.Product)
            .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

        if (inventory == null)
            return null;

        return new InventoryDto
        {
            InventoryId = inventory.InventoryId,
            ProductId = inventory.ProductId,
            Quantity = inventory.Quantity,
            UpdatedAt = inventory.UpdatedAt,
            Product = new ProductDto
            {
                ProductId = inventory.Product.ProductId,
                CategoryId = inventory.Product.CategoryId,
                SupplierId = inventory.Product.SupplierId,
                ProductName = inventory.Product.ProductName,
                Barcode = inventory.Product.Barcode,
                Price = inventory.Product.Price,
                Unit = inventory.Product.Unit,
                CreatedAt = inventory.Product.CreatedAt
            }
        };
    }

    public async Task<InventoryDto?> GetInventoryByProductIdAsync(int productId)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .ThenInclude(p => p.Category)
            .Include(i => i.Product)
            .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
            return null;

        return new InventoryDto
        {
            InventoryId = inventory.InventoryId,
            ProductId = inventory.ProductId,
            Quantity = inventory.Quantity,
            UpdatedAt = inventory.UpdatedAt,
            Product = new ProductDto
            {
                ProductId = inventory.Product.ProductId,
                CategoryId = inventory.Product.CategoryId,
                SupplierId = inventory.Product.SupplierId,
                ProductName = inventory.Product.ProductName,
                Barcode = inventory.Product.Barcode,
                Price = inventory.Product.Price,
                Unit = inventory.Product.Unit,
                CreatedAt = inventory.Product.CreatedAt
            }
        };
    }

    public async Task<InventoryDto?> UpdateInventoryAsync(int inventoryId, InventoryDto inventoryDto)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .ThenInclude(p => p.Category)
            .Include(i => i.Product)
            .ThenInclude(p => p.Supplier)
            .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

        if (inventory == null)
            return null;

        // Cập nhật thông tin
        inventory.Quantity = inventoryDto.Quantity ?? inventory.Quantity;
        inventory.UpdatedAt = DateTime.Now;

        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();

        return new InventoryDto
        {
            InventoryId = inventory.InventoryId,
            ProductId = inventory.ProductId,
            Quantity = inventory.Quantity,
            UpdatedAt = inventory.UpdatedAt,
            Product = new ProductDto
            {
                ProductId = inventory.Product.ProductId,
                CategoryId = inventory.Product.CategoryId,
                SupplierId = inventory.Product.SupplierId,
                ProductName = inventory.Product.ProductName,
                Barcode = inventory.Product.Barcode,
                Price = inventory.Product.Price,
                Unit = inventory.Product.Unit,
                CreatedAt = inventory.Product.CreatedAt
            }
        };
    }

    public async Task<bool> UpdateQuantityAsync(int productId, int quantity)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        if (inventory == null)
            return false;

        inventory.Quantity = quantity;
        inventory.UpdatedAt = (DateTime?)DateTime.Now; // Explicit cast to nullable DateTime

        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ValidateCartStockResponse> ValidateCartStockAsync(ValidateCartStockRequest request)
    {
        var response = new ValidateCartStockResponse
        {
            IsValid = true,
            OutOfStockProducts = new List<OutOfStockProduct>(),
            DeletedProducts = new List<DeletedProduct>()
        };

        foreach (var item in request.Items)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

            // Kiểm tra sản phẩm đã bị xóa mềm (Deleted = true)
            if (inventory?.Product?.Deleted == true)
            {
                response.IsValid = false;
                response.DeletedProducts.Add(new DeletedProduct
                {
                    ProductId = item.ProductId,
                    ProductName = inventory.Product.ProductName ?? "Unknown Product",
                    Quantity = item.Quantity
                });
                continue;
            }

            // Kiểm tra tồn kho
            if (inventory == null || inventory.Quantity < item.Quantity)
            {
                response.IsValid = false;
                response.OutOfStockProducts.Add(new OutOfStockProduct
                {
                    ProductId = item.ProductId,
                    ProductName = inventory?.Product?.ProductName ?? "Unknown Product",
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = inventory?.Quantity ?? 0
                });
            }
        }

        return response;
    }
}
