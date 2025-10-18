using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dotnet_backend.Dtos;
using dotnet_backend.Services.Interface;
using dotnet_backend.Database;
using dotnet_backend.Models;
namespace dotnet_backend.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Payments)
            .Include(o => o.User)
            .Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                UserId = o.UserId,
                PromoId = o.PromoId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                Status = o.Status,

                Customer = o.Customer == null ? null : new CustomerDto
                {
                    CustomerId = o.Customer.CustomerId,
                    Name = o.Customer.Name,
                    Email = o.Customer.Email,
                    Phone = o.Customer.Phone,
                    Address = o.Customer.Address,
                    CreatedAt = o.Customer.CreatedAt
                },

                Payments = o.Payments.Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod ?? "",
                    PaymentDate = p.PaymentDate ?? DateTime.MinValue
                }).ToList(),

                User = o.User == null ? null : new UserDto
                {
                    UserId = o.User.UserId,
                    Username = o.User.Username,
                    Password = "",
                    FullName = o.User.FullName,
                    Role = o.User.Role,
                    CreatedAt = o.User.CreatedAt
                }
            })
            .ToListAsync();
    }


    public async Task<OrderDto> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Payments)
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return null!;

        return new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            UserId = order.UserId,
            PromoId = order.PromoId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            Status = order.Status,

            Customer = order.Customer == null ? null : new CustomerDto
            {
                CustomerId = order.Customer.CustomerId,
                Name = order.Customer.Name,
                Email = order.Customer.Email,
                Phone = order.Customer.Phone,
                Address = order.Customer.Address,
                CreatedAt = order.Customer.CreatedAt
            },

            Payments = order.Payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod ?? "",
                PaymentDate = p.PaymentDate ?? DateTime.MinValue
            }).ToList(),

            User = order.User == null ? null : new UserDto
            {
                UserId = order.User.UserId,
                Username = order.User.Username,
                Password = "",
                FullName = order.User.FullName,
                Role = order.User.Role,
                CreatedAt = order.User.CreatedAt
            },

            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Subtotal = oi.Subtotal,
                Product = oi.Product == null ? null : new ProductDto
                {
                    ProductId = oi.Product.ProductId,
                    ProductName = oi.Product.ProductName,
                    Price = oi.Product.Price,
                    Barcode = oi.Product.Barcode ?? "",
                    Unit = oi.Product.Unit ?? "",
                    CreatedAt = oi.Product.CreatedAt,
                    CategoryId = oi.Product.CategoryId,
                    SupplierId = oi.Product.SupplierId
                }
            }).ToList()
        };
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        try
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");

            if (order.Status == "canceled")
                throw new Exception("Đơn hàng đã bị hủy trước đó.");

            // Nếu là pending thì chỉ cần đổi trạng thái, không cần hoàn kho hoặc kiểm tra ngày
            if (order.Status == "pending")
            {
                order.Status = "canceled";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return true;
            }

            // Nếu không phải pending (ví dụ: paid), thì kiểm tra ngày và hoàn kho
            if (order.OrderDate == null)
                throw new Exception("Đơn hàng không có ngày đặt hợp lệ.");

            if (order.OrderDate.Value.Date != DateTime.Now.Date)
                throw new Exception("Chỉ có thể hủy đơn hàng trong cùng ngày.");

            // Hủy đơn và hoàn kho
            order.Status = "canceled";

            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var item in orderItems)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(inv => inv.ProductId == item.ProductId);

                if (inventory == null)
                    throw new Exception($"Không tìm thấy tồn kho cho sản phẩm ID = {item.ProductId}.");

                inventory.Quantity += item.Quantity;
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi hủy đơn hàng ID {orderId}: {ex.Message}", ex);
        }
    }

    public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
    {
        orderDto.TotalAmount = 0;
        foreach (var itemDto in orderDto.OrderItems)
        {
            // ... (Logic kiểm tra giá và tạo OrderItem không đổi) ...
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null) throw new KeyNotFoundException($"Sản phẩm với ID {itemDto.ProductId} không tồn tại.");
            if (product.Price != itemDto.Price) throw new InvalidOperationException($"Giá của sản phẩm '{product.ProductName}' không chính xác.");
            itemDto.Subtotal = itemDto.Quantity * product.Price ?? 0;
            orderDto.TotalAmount += itemDto.Subtotal;
        }

        if (orderDto.PromoId == null)
        {
            orderDto.DiscountAmount = 0;
        }
        else
        {
            var promo = await _context.Promotions.FindAsync(orderDto.PromoId);
            if (promo == null)
                throw new Exception("Không tìm thấy khuyến mãi.");

            if (promo.Status != "active")
                throw new Exception("Khuyến mãi không còn hiệu lực.");

            if (promo.UsedCount >= promo.UsageLimit)
                throw new Exception("Khuyến mãi đã đạt giới hạn sử dụng.");

            if (orderDto.TotalAmount < promo.MinOrderAmount)
                throw new Exception("Tổng đơn hàng không đạt yêu cầu để áp dụng khuyến mãi.");

            if (promo.DiscountType == "percentage")
            {
                orderDto.DiscountAmount = orderDto.TotalAmount * promo.DiscountValue / 100;
            }
            else if (promo.DiscountType == "fixed")
            {
                orderDto.DiscountAmount = promo.DiscountValue;
            }
            else
            {
                throw new Exception("Loại khuyến mãi không hợp lệ.");
            }
            promo.UsedCount++;
            _context.Promotions.Update(promo);
        }
        orderDto.TotalAmount -= orderDto.DiscountAmount;

        var order = new Order
        {
            CustomerId = orderDto.CustomerId,
            UserId = orderDto.UserId,
            PromoId = orderDto.PromoId,
            OrderDate = orderDto.OrderDate,
            TotalAmount = orderDto.TotalAmount,
            DiscountAmount = orderDto.DiscountAmount,
            Status = "pending",
            OrderItems = orderDto.OrderItems.Select(oi => new OrderItem
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity ?? 1,
                Price = oi.Price,
                Subtotal = oi.Subtotal
            }).ToList()
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return await GetOrderByIdAsync(order.OrderId);
    }
}