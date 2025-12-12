using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dotnet_backend.Dtos;
using dotnet_backend.Services.Interface;
using dotnet_backend.Database;
using dotnet_backend.Models;
using dotnet_backend.Controllers; 

namespace dotnet_backend.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly PromotionService _promotionService;

    public OrderService(ApplicationDbContext context, PromotionService promotionService)
    {
        _context = context;
        _promotionService = promotionService;
    }

    public async Task<IEnumerable<PromotionDto>> GetAllPromosAsync()
    {
        return await _context.Promotions
            .Select(p => new PromotionDto
            {
                PromoId = p.PromoId,
                PromoCode = p.PromoCode,
                Description = p.Description,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                MinOrderAmount = p.MinOrderAmount,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                Status = p.Status,
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<OrderDto>> GetOrdersOfflineAsync()
    {
        return await _context.Orders
            .Where(o => o.OrderType == "offline")
            // sort by OrderDate descending (newest first)
            .OrderByDescending(o => o.OrderDate)
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
                PayStatus = o.PayStatus,
                OrderStatus = o.OrderStatus,
                OrderType = o.OrderType,

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

    public async Task<IEnumerable<OrderDto>> GetOrdersOnlineAsync()
    {
        return await _context.Orders
            .Where(o => o.OrderType == "online")
            // sort by OrderDate descending (newest first)
            .OrderByDescending(o => o.OrderDate)
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
                PayStatus = o.PayStatus,
                OrderStatus = o.OrderStatus,
                OrderType = o.OrderType,

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

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        // Deprecated: replaced by GetOrdersByCustomerIdAsync
        return Enumerable.Empty<OrderDto>();
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
    {
        // Return summary order info for a customer (no OrderItems/detail)
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
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
                PayStatus = o.PayStatus,
                OrderStatus = o.OrderStatus,
                OrderType = o.OrderType,

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

    public async Task<int> UpdateOrderAndBillStatusAsync(int orderId, string statusOrder, string statusBill)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Cập nhật trạng thái ORDER
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                throw new Exception("Khong tim thay don hang");

            order.PayStatus = statusOrder;

            // Cập nhật trạng thái BILL
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.OrderId == orderId);

            if (bill == null)
                throw new Exception("Khong tim thay hoa don");

            bill.PayStatus = statusBill;

            if (statusBill == "paid")
            {
                bill.PaidAt = DateTime.Now;
            }

            // Lưu thay đổi
            int result = await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            return result; // số dòng bị ảnh hưởng
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOnlineOrdersByCustomerIdAsync(int customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId && o.OrderType == "online")
            .OrderByDescending(o => o.OrderDate)
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
                PayStatus = o.PayStatus,
                OrderStatus = o.OrderStatus,
                OrderType = o.OrderType,

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

    public async Task<IEnumerable<PeakTimeDto>> GetPeakTimeStatsAsync()
    {
        var result = await _context.Orders
            .Where(o => o.OrderDate.HasValue)
            .Select(o => o.OrderDate.Value.TimeOfDay)
            .ToListAsync();

        if (!result.Any()) return new List<PeakTimeDto>();

        int morning = result.Count(t => t >= new TimeSpan(7, 0, 0) && t < new TimeSpan(11, 30, 0));
        int afternoon = result.Count(t => t >= new TimeSpan(11, 30, 0) && t < new TimeSpan(17, 0, 0));
        int evening = result.Count(t => t >= new TimeSpan(17, 0, 0) && t < new TimeSpan(20, 30, 0));

        int total = morning + afternoon + evening;

        if (total == 0) return new List<PeakTimeDto>();

        return new List<PeakTimeDto>
        {
            new PeakTimeDto { TimeRange = "07:00 - 11:30", Percentage = Math.Round((decimal)morning / total * 100, 2) },
            new PeakTimeDto { TimeRange = "11:30 - 17:00", Percentage = Math.Round((decimal)afternoon / total * 100, 2) },
            new PeakTimeDto { TimeRange = "17:00 - 20:30", Percentage = Math.Round((decimal)evening / total * 100, 2) },
        };
    }

    public async Task<int> GetTotalOrdersAsync()
    {
        return await _context.Orders.CountAsync();
    }


    public async Task<IEnumerable<OrderByMonthDto>> GetOrdersByYearAsync(int year)
    {
        var result = await _context.Orders
                    .Where(o => o.OrderDate.Value.Year == year)
                    .GroupBy(o => o.OrderDate.Value.Month)
                    .Select(g => new OrderByMonthDto
                    {
                        Month = g.Key,
                        TotalOrders = g.Count()
                    })
                    .OrderBy(o => o.Month)
                    .ToListAsync();



        var fullYear = Enumerable.Range(1, 12)
            .GroupJoin(result,
                m => m,
                r => r.Month,
                (m, r) => new OrderByMonthDto
                {
                    Month = m,
                    TotalOrders = r.FirstOrDefault()?.TotalOrders ?? 0
                })
            .ToList();

        return fullYear;
    }

    public async Task<IEnumerable<SalesByMonthDto>> GetSalesByYearAsync(int year)
    {
        var result = await _context.Orders
            .Where(o => o.OrderDate.Value.Year == year)
            .GroupBy(o => o.OrderDate.Value.Month)
            .Select(g => new SalesByMonthDto
            {
                Month = g.Key,
                TotalSales = g.Sum(o => o.TotalAmount ?? 0)
            })
            .OrderBy(o => o.Month)
            .ToListAsync();

        var fullYear = Enumerable.Range(1, 12)
            .GroupJoin(result,
                m => m,
                r => r.Month,
                (m, r) => new SalesByMonthDto
                {
                    Month = m,
                    TotalSales = r.FirstOrDefault()?.TotalSales ?? 0
                })
            .ToList();

        return fullYear;
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
            PromoId = order.PromoId,          // <- thêm
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,  // <- thêm
            DiscountAmount = order.DiscountAmount, // <- thêm
            PayStatus = order.PayStatus,
            OrderStatus = order.OrderStatus,
            OrderType = order.OrderType,

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
                throw new Exception("Khong tim thay don hang");

            if (order.PayStatus == "canceled")
                throw new Exception("Don hang da bi huy truoc do");

            // Nếu là pending thì chỉ cần đổi trạng thái, không cần hoàn kho hoặc kiểm tra ngày
            if (order.PayStatus == "pending")
            {
                order.PayStatus = "canceled";
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
            order.PayStatus = "canceled";

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
        // Require customerId provided by frontend. Accept 0 or any integer, but ensure the customer exists.
        if (orderDto.CustomerId == null)
        {
            throw new ArgumentException("customerId là bắt buộc.");
        }

        var customer = await _context.Customers.FindAsync(orderDto.CustomerId.Value);
        if (customer == null)
        {
            throw new ArgumentException("customerId không tồn tại trong hệ thống.");
        }
        if (orderDto.OrderItems.Count == 0)
        {
            throw new ArgumentException("Đơn hàng phải có ít nhất một món.");
        }
        orderDto.TotalAmount = 0;
        foreach (var itemDto in orderDto.OrderItems)
        {
            // ... (Logic kiểm tra giá và tạo OrderItem không đổi) ...
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null) throw new ArgumentException($"Sản phẩm với ID {itemDto.ProductId} không tồn tại.");
            if (product.Price != itemDto.Price) throw new ArgumentException($"Giá của sản phẩm '{product.ProductName}' không chính xác.");
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
                throw new ArgumentException("Không tìm thấy khuyến mãi.");

            if (promo.Status != "active")
                throw new ArgumentException("Khuyến mãi không còn hiệu lực.");

            if (promo.UsedCount >= promo.UsageLimit)
                throw new ArgumentException("Khuyến mãi đã đạt giới hạn sử dụng.");

            if (orderDto.TotalAmount < promo.MinOrderAmount)
                throw new ArgumentException("Tổng đơn hàng không đạt yêu cầu để áp dụng khuyến mãi.");

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
                throw new ArgumentException("Loại khuyến mãi không hợp lệ.");
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
            PayStatus = "pending",
            OrderType = orderDto.OrderType ?? "offline",
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
    // Implement interface
    public async Task<OrderDto> PreviewOrderFromCartAsync(int customerId, int? promoId = null, string paymentMethod = "cash")
    {
        // Build order DTO from current cart without persisting or modifying DB
        // 1. Lấy giỏ hàng
        var cartItems = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.CustomerId == customerId)
            .ToListAsync();

        if (!cartItems.Any())
            throw new ArgumentException("Giỏ hàng trống.");

        // 2. Kiểm tra tồn kho (chỉ kiểm tra, không thay đổi)
        foreach (var ci in cartItems)
        {
            var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == ci.ProductId);
            if (inv == null) throw new Exception($"Không tìm thấy tồn kho cho sản phẩm {ci.Product.ProductName}");
            if (inv.Quantity < ci.Quantity) throw new Exception($"Sản phẩm {ci.Product.ProductName} chỉ còn {inv.Quantity} trong kho.");
        }

        // 3. Tạo OrderItems
        var orderItems = cartItems.Select(ci => new OrderItemDto
        {
            ProductId = ci.ProductId,
            Quantity = ci.Quantity,
            Price = ci.Price,
            Subtotal = ci.Subtotal,
            Product = ci.Product == null ? null : new ProductDto
            {
                ProductId = ci.Product.ProductId,
                ProductName = ci.Product.ProductName,
                Price = ci.Product.Price
            }
        }).ToList();

        var total = orderItems.Sum(x => x.Subtotal);

        // 4. Tính discount nếu có
        decimal discount = 0;
        int? appliedPromoId = null;
        if (promoId.HasValue)
        {
            var promoResp = await _promotionService.ValidatePromoAsync(promoId.Value, total);
            discount = promoResp.DiscountAmount;
            appliedPromoId = promoResp.PromoId;
        }

        // 5. Build OrderDto
        var orderDto = new OrderDto
        {
            CustomerId = customerId,
            PromoId = appliedPromoId,
            OrderDate = DateTime.Now,
            TotalAmount = total,
            DiscountAmount = discount,
            PayStatus = "pending",
            OrderItems = orderItems
        };

        return orderDto;
    }

    public async Task<OrderDto> CheckoutFromCartAsync(int customerId, int? userId = null, int? promoId = null)
    {
        string? promoCode = null;

        if (promoId.HasValue)
        {
            var promo = await _context.Promotions.FindAsync(promoId.Value);
            promoCode = promo?.PromoCode;
        }

        return await CheckoutFromCartInternalAsync(customerId, userId, promoCode, "cash");
    }

    
    // OrderService
    public async Task<OrderDto> CheckoutFromCartAsync(CheckoutDto checkout)
    {
        if (checkout == null) throw new ArgumentNullException(nameof(checkout));

        return await CheckoutFromCartInternalAsync(
            checkout.CustomerId,
            null,
            checkout.PromoCode,
            checkout.PaymentMethod ?? "cash"
        );
    }

    // Nếu muốn dùng từ backend theo CustomerId, UserId
    public async Task<OrderDto> CheckoutFromCartAsync(int customerId, int? userId, string? promoCode, string paymentMethod = "cash")
    {
        return await CheckoutFromCartInternalAsync(customerId, userId, promoCode, paymentMethod);
    }

    // Hàm private xử lý chung
    private async Task<OrderDto> CheckoutFromCartInternalAsync(
        int customerId,
        int? userId,
        string? promoCode,
        string paymentMethod)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Lấy giỏ hàng
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
                throw new ArgumentException("Giỏ hàng trống.");

            // 2. Kiểm tra tồn kho
            foreach (var ci in cartItems)
            {
                var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == ci.ProductId);
                if (inv == null) throw new Exception($"Không tìm thấy tồn kho cho sản phẩm {ci.Product.ProductName}");
                if (inv.Quantity < ci.Quantity) throw new Exception($"Sản phẩm {ci.Product.ProductName} chỉ còn {inv.Quantity} trong kho.");
            }

            // 3. Tạo OrderItems
            var orderItems = cartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Price,
                Subtotal = ci.Subtotal
            }).ToList();

            var total = orderItems.Sum(x => x.Subtotal);

            // 4. Tính discount
            decimal discount = 0;
            int? appliedPromoId = null;

            if (!string.IsNullOrWhiteSpace(promoCode))
            {
                var promoResponse = await _promotionService.ValidatePromoByCodeAsync(promoCode, total);
                discount = promoResponse.DiscountAmount;
                appliedPromoId = promoResponse.PromoId;

                if (appliedPromoId.HasValue)
                {
                    var promo = await _context.Promotions.FindAsync(appliedPromoId.Value);
                    if (promo != null)
                    {
                        promo.UsedCount = (promo.UsedCount ?? 0) + 1;
                        _context.Promotions.Update(promo);
                    }
                }
            }

            var finalTotal = total - discount;

            // 5. Tạo Order
            var order = new Order
            {
                CustomerId     = customerId,
                UserId         = userId,
                PromoId        = appliedPromoId,
                OrderDate      = DateTime.Now,
                TotalAmount    = total,
                DiscountAmount = discount,
                PayStatus      = "pending",
                OrderItems     = orderItems,
                OrderType      = "online"
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 6. Tạo Payment
            var payment = new Payment
            {
                OrderId       = order.OrderId,
                Amount        = finalTotal,
                PaymentMethod = paymentMethod,
                PaymentDate   = DateTime.Now
            };
            _context.Payments.Add(payment);
            // 7. Tạo Bill
            var bill = new Bill
            {
                OrderId        = order.OrderId,
                CustomerId     = customerId,
                TotalAmount    = total,
                DiscountAmount = discount,
                FinalAmount    = finalTotal,
                PaymentMethod  = paymentMethod,
                PayStatus      = "unpaid",
                CreatedAt      = DateTime.Now
            };

            // If payment method is immediate (cash or e-wallet), mark order and bill as paid now
            var method = (paymentMethod ?? "").ToLower();
            if (method == "cash" || method == "e-wallet")
            {
                order.PayStatus = "paid";
                bill.PayStatus = "paid";
                bill.PaidAt = DateTime.Now;
                // also set payment date if not set
                payment.PaymentDate = DateTime.Now;
            }

            _context.Bills.Add(bill);

            // 8. Cập nhật tồn kho
            foreach (var ci in cartItems)
            {
                var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == ci.ProductId);
                if (inv != null)
                {
                    inv.Quantity -= ci.Quantity;
                    _context.Inventories.Update(inv);
                }
            }

            // 9. Xóa cart items
            _context.CartItems.RemoveRange(cartItems);

            // 10. Lưu tất cả thay đổi
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return await GetOrderByIdAsync(order.OrderId);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            Console.WriteLine($"[ERROR] Checkout failed: {ex.Message}");
            throw;
        }
    }
}