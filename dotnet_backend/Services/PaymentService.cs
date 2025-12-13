using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;
using dotnet_backend.Database;
using dotnet_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto> CreatePayment(PaymentDto paymentDto)
    {
        Console.WriteLine($"[PaymentService] CreatePayment called for OrderId={paymentDto.OrderId}, Amount={paymentDto.Amount}, Method={paymentDto.PaymentMethod}");

        if (paymentDto == null)
            throw new ArgumentException("Dữ liệu payment không hợp lệ.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == paymentDto.OrderId);

            if (order == null)
                throw new Exception("Khong tim thay order.");

            Console.WriteLine($"[PaymentService] Order found: PayStatus={order.PayStatus}, TotalAmount={order.TotalAmount}");

            var paidBefore = await _context.Payments
                .Where(p => p.OrderId == order.OrderId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var afterPay = paidBefore + paymentDto.Amount;
            Console.WriteLine($"[PaymentService] PaidBefore={paidBefore}, AfterPay={afterPay}");

            if (afterPay > order.TotalAmount)
                throw new Exception("Thanh toán vượt tổng cần trả.");

            // kiểm tra tồn kho
            foreach (var oi in order.OrderItems)
            {
                var inv = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == oi.ProductId);

                if (inv == null)
                    throw new Exception($"Không có tồn kho cho product {oi.ProductId}");

                if (inv.Quantity < oi.Quantity)
                    throw new Exception($"Không đủ tồn kho cho sản phẩm {oi.ProductId}");

                Console.WriteLine($"[PaymentService] Inventory check OK for ProductId={oi.ProductId}, Quantity={inv.Quantity}");
            }

            // tạo payment
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = paymentDto.Amount,
                PaymentMethod = paymentDto.PaymentMethod,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(payment);
            Console.WriteLine($"[PaymentService] Payment added to context");

            // If payment method is cash or e-wallet, mark order paid immediately.
            var method = (paymentDto.PaymentMethod ?? "").ToLower();
            if (method == "cash" || method == "e-wallet")
            {
                order.PayStatus = "paid";
                _context.Orders.Update(order);
                Console.WriteLine($"[PaymentService] Order status updated to 'paid' due to immediate payment method: {paymentDto.PaymentMethod}");

                foreach (var oi in order.OrderItems)
                {
                    var inv = await _context.Inventories.FirstAsync(i => i.ProductId == oi.ProductId);
                    inv.Quantity -= oi.Quantity;
                    inv.UpdatedAt = DateTime.Now;
                    _context.Inventories.Update(inv);
                    Console.WriteLine($"[PaymentService] Inventory updated: ProductId={oi.ProductId}, NewQty={inv.Quantity}");
                }

                var bill = new Bill
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    TotalAmount = order.TotalAmount ?? 0,
                    DiscountAmount = order.DiscountAmount ?? 0,
                    FinalAmount = (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0),
                    PaymentMethod = paymentDto.PaymentMethod,
                    PayStatus = "paid",
                    CreatedAt = DateTime.Now,
                    PaidAt = DateTime.Now,
                    Name = order.Name,
                    Address = order.Address,
                    Phone = order.Phone,
                    Email = order.Email
                };
                _context.Bills.Add(bill);
                Console.WriteLine($"[PaymentService] Bill created for OrderId={order.OrderId}");
            }
            else if (afterPay == order.TotalAmount)
            {
                // fallback: if full amount paid by other method, mark paid
                order.PayStatus = "paid";
                _context.Orders.Update(order);
                Console.WriteLine($"[PaymentService] Order status updated to 'paid' (full payment)");

                foreach (var oi in order.OrderItems)
                {
                    var inv = await _context.Inventories.FirstAsync(i => i.ProductId == oi.ProductId);
                    inv.Quantity -= oi.Quantity;
                    inv.UpdatedAt = DateTime.Now;
                    _context.Inventories.Update(inv);
                    Console.WriteLine($"[PaymentService] Inventory updated: ProductId={oi.ProductId}, NewQty={inv.Quantity}");
                }

                var bill = new Bill
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    TotalAmount = order.TotalAmount ?? 0,
                    DiscountAmount = order.DiscountAmount ?? 0,
                    FinalAmount = (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0),
                    PaymentMethod = paymentDto.PaymentMethod,
                    PayStatus = "paid",
                    CreatedAt = DateTime.Now,
                    PaidAt = DateTime.Now,
                    Name = order.Name,
                    Address = order.Address,
                    Phone = order.Phone,
                    Email = order.Email
                };
                _context.Bills.Add(bill);
                Console.WriteLine($"[PaymentService] Bill created for OrderId={order.OrderId}");
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            Console.WriteLine($"[PaymentService] Changes saved to database");

            paymentDto.PaymentId = payment.PaymentId;
            paymentDto.PaymentDate = payment.PaymentDate ?? DateTime.Now;

            return paymentDto;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            Console.WriteLine($"[PaymentService] Exception: {ex.Message}");
            throw;
        }
    }
}
