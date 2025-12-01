using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    /// <summary>
    /// API hóa đơn cho customer (cần đăng nhập)
    /// </summary>
    [ApiController]
    [Route("api/customer/bills")]
    [Authorize]
    public class CustomerBillController : ControllerBase
    {
        private readonly IBillService _billService;

        public CustomerBillController(IBillService billService)
        {
            _billService = billService;
        }

        /// <summary>
        /// Lấy customerId từ JWT token
        /// </summary>
        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("customer_id")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
                throw new UnauthorizedAccessException("Token không hợp lệ");
            return customerId;
        }

        /// <summary>
        /// Lấy tất cả hóa đơn của mình
        /// GET: api/customer/bills
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyBills()
        {
            try
            {
                var customerId = GetCustomerId();
                var bills = await _billService.GetBillsByCustomerIdAsync(customerId);
                return Ok(bills);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn
        /// GET: api/customer/bills/{billId}
        /// </summary>
        [HttpGet("{billId}")]
        public async Task<IActionResult> GetBillById(int billId)
        {
            try
            {
                var customerId = GetCustomerId();
                var bill = await _billService.GetBillByIdAsync(billId);
                
                if (bill == null)
                    return NotFound(new { message = "Không tìm thấy hóa đơn" });

                // Kiểm tra xem bill có thuộc về customer này không
                if (bill.CustomerId != customerId)
                    return Forbid();

                return Ok(bill);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy hóa đơn theo order ID
        /// GET: api/customer/bills/order/{orderId}
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetBillByOrderId(int orderId)
        {
            try
            {
                var customerId = GetCustomerId();
                var bill = await _billService.GetBillByOrderIdAsync(orderId);
                
                if (bill == null)
                    return NotFound(new { message = "Không tìm thấy hóa đơn cho đơn hàng này" });

                // Kiểm tra xem bill có thuộc về customer này không
                if (bill.CustomerId != customerId)
                    return Forbid();

                return Ok(bill);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy hóa đơn theo trạng thái
        /// GET: api/customer/bills/status/{status}
        /// Status: unpaid, paid, cancelled
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetBillsByStatus(string status)
        {
            try
            {
                var customerId = GetCustomerId();
                var allBills = await _billService.GetBillsByCustomerIdAsync(customerId);
                
                var filteredBills = allBills.Where(b => 
                    b.Status?.ToLower() == status.ToLower()).ToList();

                return Ok(filteredBills);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy hóa đơn chưa thanh toán
        /// GET: api/customer/bills/unpaid
        /// </summary>
        [HttpGet("unpaid")]
        public async Task<IActionResult> GetUnpaidBills()
        {
            try
            {
                var customerId = GetCustomerId();
                var bills = await _billService.GetBillsByCustomerIdAsync(customerId);
                
                var unpaidBills = bills.Where(b => b.Status == "unpaid").ToList();
                return Ok(unpaidBills);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử hóa đơn đã thanh toán
        /// GET: api/customer/bills/paid
        /// </summary>
        [HttpGet("paid")]
        public async Task<IActionResult> GetPaidBills()
        {
            try
            {
                var customerId = GetCustomerId();
                var bills = await _billService.GetBillsByCustomerIdAsync(customerId);
                
                var paidBills = bills.Where(b => b.Status == "paid")
                    .OrderByDescending(b => b.PaidAt)
                    .ToList();
                
                return Ok(paidBills);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tính tổng số tiền đã chi tiêu
        /// GET: api/customer/bills/total-spent
        /// </summary>
        [HttpGet("total-spent")]
        public async Task<IActionResult> GetTotalSpent()
        {
            try
            {
                var customerId = GetCustomerId();
                var bills = await _billService.GetBillsByCustomerIdAsync(customerId);
                
                var totalSpent = bills
                    .Where(b => b.Status == "paid")
                    .Sum(b => b.FinalAmount);

                return Ok(new { totalSpent });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
