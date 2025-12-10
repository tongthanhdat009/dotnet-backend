using Microsoft.EntityFrameworkCore;
using dotnet_backend.Database;
using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using dotnet_backend.Dtos;
namespace dotnet_backend.Services;

public class OrderItemService : IOrderItemService
{
    private readonly ApplicationDbContext _context;

    public OrderItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderItemWithProductDto>> GetOrderItemsWithProductsAsync(int orderId)
    {
        return await _context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .Join(
                _context.Products,
                oi => oi.ProductId,
                p => p.ProductId,
                (oi, p) => new OrderItemWithProductDto
                {
                    OrderItemId = oi.OrderItemId,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Subtotal = oi.Subtotal,

                    ProductName = p.ProductName,
                    Barcode = p.Barcode,
                    ProductPrice = p.Price,
                    Unit = p.Unit
                }
            )
            .ToListAsync();
    }

}