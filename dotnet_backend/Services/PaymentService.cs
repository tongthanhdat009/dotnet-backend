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
        // Basic validation moved to service
        if (paymentDto == null)
            throw new ArgumentException("Dữ liệu payment không hợp lệ.");

        var allowedMethods = new[] { "cash", "card", "bank_transfer", "e-wallet" };

        if (paymentDto.Amount <= 0)
            throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0.");

        if (string.IsNullOrWhiteSpace(paymentDto.PaymentMethod) || !allowedMethods.Contains(paymentDto.PaymentMethod))
            throw new ArgumentException($"Phương thức thanh toán không hợp lệ: {paymentDto.PaymentMethod}");

        var order = await _context.Orders.FindAsync(paymentDto.OrderId);
        if (order == null)
        {
            throw new ArgumentException("Đơn hàng không tồn tại");
        }
        var payments = await _context.Payments
            .Where(p => p.OrderId == paymentDto.OrderId)
            .ToListAsync();
        var totalPaid = payments.Sum(p => p.Amount);
        if (totalPaid + paymentDto.Amount > order.TotalAmount)
        {
            throw new ArgumentException("Số tiền thanh toán vượt quá tổng số tiền của đơn hàng");
        }
        var payment = new Payment
        {
            OrderId = paymentDto.OrderId,
            Amount = paymentDto.Amount,
            PaymentMethod = paymentDto.PaymentMethod,
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        if (paymentDto.Amount + totalPaid == order.TotalAmount)
        {
            order.Status = "paid";
            _context.Orders.Update(order);
        }
        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == paymentDto.OrderId)
            .ToListAsync();

        foreach (var orderItem in orderItems)
        {
            var inventory = await _context.Inventories
                .Where(inv => inv.ProductId == orderItem.ProductId)
                .FirstOrDefaultAsync();
            if (inventory != null)
            {
                inventory.Quantity -= orderItem.Quantity;
                _context.Inventories.Update(inventory);
            }
        }

        await _context.SaveChangesAsync();
        paymentDto.PaymentId = payment.PaymentId;
        return paymentDto;
    }
}