using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using System;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillController : ControllerBase
    {
        private readonly IBillService _billService;

        public BillController(IBillService billService)
        {
            _billService = billService;
        }

        /// <summary>
        /// Tạo hóa đơn từ order
        /// POST: api/bill/create-from-order/{orderId}
        /// </summary>
        [HttpPost("create-from-order/{orderId}")]
        public async Task<IActionResult> CreateBillFromOrder(int orderId)
        {
            try
            {
                var bill = await _billService.CreateBillFromOrderAsync(orderId);
                return Ok(new { message = "Tạo hóa đơn thành công", data = bill });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy hóa đơn theo ID
        /// GET: api/bill/{billId}
        /// </summary>
        [HttpGet("{billId}")]
        public async Task<IActionResult> GetBillById(int billId)
        {
            var bill = await _billService.GetBillByIdAsync(billId);
            if (bill == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn" });

            return Ok(bill);
        }

        /// <summary>
        /// Lấy tất cả hóa đơn
        /// GET: api/bill
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllBills()
        {
            var bills = await _billService.GetAllBillsAsync();
            return Ok(bills);
        }

        /// <summary>
        /// Lấy hóa đơn của khách hàng
        /// GET: api/bill/customer/{customerId}
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetBillsByCustomerId(int customerId)
        {
            var bills = await _billService.GetBillsByCustomerIdAsync(customerId);
            return Ok(bills);
        }

        /// <summary>
        /// Lấy hóa đơn theo Order ID
        /// GET: api/bill/order/{orderId}
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetBillByOrderId(int orderId)
        {
            var bill = await _billService.GetBillByOrderIdAsync(orderId);
            if (bill == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn cho order này" });

            return Ok(bill);
        }

        /// <summary>
        /// Cập nhật trạng thái hóa đơn
        /// PUT: api/bill/{billId}/status
        /// Body: { "status": "paid" }
        /// </summary>
        [HttpPut("{billId}/status")]
        public async Task<IActionResult> UpdateBillStatus(int billId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var bill = await _billService.UpdateBillStatusAsync(billId, request.Status);
                if (bill == null)
                    return NotFound(new { message = "Không tìm thấy hóa đơn" });

                return Ok(new { message = "Cập nhật trạng thái thành công", data = bill });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán
        /// POST: api/bill/{billId}/pay
        /// Body: { "paymentMethod": "cash" }
        /// </summary>
        [HttpPost("{billId}/pay")]
        public async Task<IActionResult> MarkBillAsPaid(int billId, [FromBody] PaymentRequest request)
        {
            var bill = await _billService.MarkBillAsPaidAsync(billId, request.PaymentMethod);
            if (bill == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn" });

            return Ok(new { message = "Đã thanh toán hóa đơn", data = bill });
        }

        /// <summary>
        /// Hủy hóa đơn
        /// POST: api/bill/{billId}/cancel
        /// </summary>
        [HttpPost("{billId}/cancel")]
        public async Task<IActionResult> CancelBill(int billId)
        {
            try
            {
                var bill = await _billService.CancelBillAsync(billId);
                if (bill == null)
                    return NotFound(new { message = "Không tìm thấy hóa đơn" });

                return Ok(new { message = "Đã hủy hóa đơn", data = bill });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa hóa đơn
        /// DELETE: api/bill/{billId}
        /// </summary>
        [HttpDelete("{billId}")]
        public async Task<IActionResult> DeleteBill(int billId)
        {
            try
            {
                var result = await _billService.DeleteBillAsync(billId);
                if (!result)
                    return NotFound(new { message = "Không tìm thấy hóa đơn" });

                return Ok(new { message = "Đã xóa hóa đơn" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lọc hóa đơn theo trạng thái
        /// GET: api/bill/status/{status}
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetBillsByStatus(string status)
        {
            var bills = await _billService.GetBillsByStatusAsync(status);
            return Ok(bills);
        }

        /// <summary>
        /// Lấy hóa đơn theo khoảng thời gian
        /// GET: api/bill/date-range?startDate=2024-01-01&endDate=2024-12-31
        /// </summary>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetBillsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var bills = await _billService.GetBillsByDateRangeAsync(startDate, endDate);
            return Ok(bills);
        }

        /// <summary>
        /// Tính tổng doanh thu
        /// GET: api/bill/revenue/total
        /// </summary>
        [HttpGet("revenue/total")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var revenue = await _billService.GetTotalRevenueAsync();
            return Ok(new { totalRevenue = revenue });
        }

        /// <summary>
        /// Tính doanh thu theo khoảng thời gian
        /// GET: api/bill/revenue/date-range?startDate=2024-01-01&endDate=2024-12-31
        /// </summary>
        [HttpGet("revenue/date-range")]
        public async Task<IActionResult> GetRevenueByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var revenue = await _billService.GetRevenueByDateRangeAsync(startDate, endDate);
            return Ok(new { revenue, startDate, endDate });
        }
    }

    // Request models
    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentRequest
    {
        public string PaymentMethod { get; set; } = "cash";
    }
}
