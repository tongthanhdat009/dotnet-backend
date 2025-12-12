using dotnet_backend.Database;
using dotnet_backend.Models;
using dotnet_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_backend.Services
{
    public class BillService : IBillService
    {
        private readonly ApplicationDbContext _context;

        public BillService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo hóa đơn mới từ order
        /// </summary>
        public async Task<Bill> CreateBillFromOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                throw new ArgumentException("Order không tồn tại", nameof(orderId));

            if (order.CustomerId == null)
                throw new ArgumentException("Order không có thông tin khách hàng", nameof(orderId));

            // Kiểm tra xem đã có bill cho order này chưa
            var existingBill = await _context.Bills
                .FirstOrDefaultAsync(b => b.OrderId == orderId);

            if (existingBill != null)
                throw new InvalidOperationException("Order này đã có hóa đơn");

            var bill = new Bill
            {
                OrderId = orderId,
                CustomerId = order.CustomerId.Value,
                TotalAmount = order.TotalAmount ?? 0,
                DiscountAmount = order.DiscountAmount ?? 0,
                FinalAmount = (order.TotalAmount ?? 0) - (order.DiscountAmount ?? 0),
                PayStatus = "paid",
                CreatedAt = DateTime.Now
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return bill;
        }

        /// <summary>
        /// Lấy hóa đơn theo ID
        /// </summary>
        public async Task<Bill?> GetBillByIdAsync(int billId)
        {
            return await _context.Bills
                .Include(b => b.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.BillId == billId);
        }

        /// <summary>
        /// Lấy tất cả hóa đơn
        /// </summary>
        public async Task<List<Bill>> GetAllBillsAsync()
        {
            return await _context.Bills
                .Include(b => b.Order)
                .Include(b => b.Customer)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy hóa đơn của khách hàng
        /// </summary>
        public async Task<List<Bill>> GetBillsByCustomerIdAsync(int customerId)
        {
            return await _context.Bills
                .Include(b => b.Order)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy hóa đơn theo Order ID
        /// </summary>
        public async Task<Bill?> GetBillByOrderIdAsync(int orderId)
        {
            return await _context.Bills
                .Include(b => b.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.OrderId == orderId);
        }

        /// <summary>
        /// Cập nhật trạng thái hóa đơn
        /// </summary>
        public async Task<Bill?> UpdateBillStatusAsync(int billId, string status)
        {
            var validStatuses = new[] { "unpaid", "paid", "cancelled" };
            if (!validStatuses.Contains(status.ToLower()))
                throw new ArgumentException("Trạng thái không hợp lệ. Chỉ chấp nhận: unpaid, paid, cancelled", nameof(status));

            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null)
                return null;

            bill.PayStatus = status.ToLower();

            if (status.ToLower() == "paid" && bill.PaidAt == null)
            {
                bill.PaidAt = DateTime.Now;
            }

            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();

            return bill;
        }

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán
        /// </summary>
        public async Task<Bill?> MarkBillAsPaidAsync(int billId, string paymentMethod)
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null)
                return null;

            bill.PayStatus = "paid";
            bill.PaymentMethod = paymentMethod;
            bill.PaidAt = DateTime.Now;

            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();

            return bill;
        }

        /// <summary>
        /// Hủy hóa đơn
        /// </summary>
        public async Task<Bill?> CancelBillAsync(int billId)
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null)
                return null;

            if (bill.PayStatus == "paid")
                throw new InvalidOperationException("Khong the huy hoa don da thanh toan");

            bill.PayStatus = "cancelled";

            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();

            return bill;
        }

        /// <summary>
        /// Xóa hóa đơn
        /// </summary>
        public async Task<bool> DeleteBillAsync(int billId)
        {
            var bill = await _context.Bills.FindAsync(billId);
            if (bill == null)
                return false;

            if (bill.PayStatus == "paid")
                throw new InvalidOperationException("Khong the xoa hoa don da thanh toan");

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Lọc hóa đơn theo trạng thái
        /// </summary>
        public async Task<List<Bill>> GetBillsByStatusAsync(string status)
        {
            return await _context.Bills
                .Include(b => b.Order)
                .Include(b => b.Customer)
                .Where(b => b.PayStatus == status.ToLower())
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy hóa đơn theo khoảng thời gian
        /// </summary>
        public async Task<List<Bill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Bills
                .Include(b => b.Order)
                .Include(b => b.Customer)
                .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Tính tổng doanh thu từ hóa đơn đã thanh toán
        /// </summary>
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Bills
                .Where(b => b.PayStatus == "paid")
                .SumAsync(b => b.FinalAmount);
        }

        /// <summary>
        /// Tính tổng doanh thu theo khoảng thời gian
        /// </summary>
        public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Bills
                .Where(b => b.PayStatus == "paid" 
                    && b.PaidAt >= startDate 
                    && b.PaidAt <= endDate)
                .SumAsync(b => b.FinalAmount);
        }
    }
}
