using dotnet_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_backend.Services.Interface
{
    public interface IBillService
    {
        // Tạo hóa đơn mới từ order
        Task<Bill> CreateBillFromOrderAsync(int orderId);

        // Lấy hóa đơn theo ID
        Task<Bill?> GetBillByIdAsync(int billId);

        // Lấy tất cả hóa đơn
        Task<List<Bill>> GetAllBillsAsync();

        // Lấy hóa đơn của khách hàng
        Task<List<Bill>> GetBillsByCustomerIdAsync(int customerId);

        // Lấy hóa đơn theo Order ID
        Task<Bill?> GetBillByOrderIdAsync(int orderId);

        // Cập nhật trạng thái hóa đơn
        Task<Bill?> UpdateBillStatusAsync(int billId, string status);

        // Đánh dấu hóa đơn đã thanh toán
        Task<Bill?> MarkBillAsPaidAsync(int billId, string paymentMethod);

        // Hủy hóa đơn
        Task<Bill?> CancelBillAsync(int billId);

        // Xóa hóa đơn
        Task<bool> DeleteBillAsync(int billId);

        // Lọc hóa đơn theo trạng thái
        Task<List<Bill>> GetBillsByStatusAsync(string status);

        // Lấy hóa đơn theo khoảng thời gian
        Task<List<Bill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Tính tổng doanh thu từ hóa đơn đã thanh toán
        Task<decimal> GetTotalRevenueAsync();

        // Tính tổng doanh thu theo khoảng thời gian
        Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
