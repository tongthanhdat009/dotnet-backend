using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;
using dotnet_backend.Database;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/customer/vnpay")]
    [Authorize]
    public class CustomerVNPayController : ControllerBase
    {
        private readonly IVNPayService _vnpayService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CustomerVNPayController(IVNPayService vnpayService, ApplicationDbContext context, IConfiguration configuration)
        {
            _vnpayService = vnpayService;
            _context = context;
            _configuration = configuration;
        }

        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("customer_id")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
                throw new UnauthorizedAccessException("Token không hợp lệ");
            return customerId;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] VNPayRequestDto request)
        {
            try
            {
                var customerId = GetCustomerId();

                // Kiểm tra đơn hàng có thuộc về customer này không
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.CustomerId == customerId);

                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });
                }

                // Kiểm tra trạng thái đơn hàng
                if (order.PayStatus == "paid")
                {
                    return BadRequest(new { message = "Đơn hàng đã được thanh toán" });
                }

                // Lấy IP address
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

                // Tạo URL thanh toán
                var paymentUrl = _vnpayService.CreatePaymentUrl(request, ipAddress);

                return Ok(new VNPayResponseDto
                {
                    Success = true,
                    PaymentUrl = paymentUrl,
                    Message = "Tạo URL thanh toán thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new VNPayResponseDto
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xử lý callback từ VNPay (không cần authorization vì VNPay gọi trực tiếp)
        /// </summary>
        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                // Lấy tất cả query parameters
                var vnpayData = Request.Query.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToString()
                );

                // Xử lý callback
                var result = await _vnpayService.ProcessCallbackAsync(vnpayData);

                // Redirect về trang kết quả trong Blazor app
                var blazorAppUrl = _configuration.GetSection("VNPay")["BlazorAppUrl"] ?? "http://localhost:5192";
                var blazorUrl = $"{blazorAppUrl}/payment-result?" +
                    $"success={result.Success}&" +
                    $"message={Uri.EscapeDataString(result.Message ?? "")}&" +
                    $"orderId={result.OrderId}&" +
                    $"transactionId={result.TransactionId}&" +
                    $"amount={result.Amount}";

                return Redirect(blazorUrl);
            }
            catch (Exception ex)
            {
                var blazorAppUrl = _configuration.GetSection("VNPay")["BlazorAppUrl"] ?? "http://localhost:5192";
                var blazorUrl = $"{blazorAppUrl}/payment-result?" +
                    $"success=false&" +
                    $"message={Uri.EscapeDataString($"Lỗi: {ex.Message}")}";

                return Redirect(blazorUrl);
            }
        }

        /// <summary>
        /// API để lấy thông tin callback (cho Blazor app gọi)
        /// </summary>
        [HttpGet("verify-payment")]
        [AllowAnonymous]
        public IActionResult VerifyPayment([FromQuery] Dictionary<string, string> vnpayData)
        {
            try
            {
                var secureHash = vnpayData.GetValueOrDefault("vnp_SecureHash");
                var isValid = _vnpayService.ValidateSignature(vnpayData, secureHash);

                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
