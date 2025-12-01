using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using System.Security.Claims;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/customer/auth")]
    public class CustomerAuthController : ControllerBase
    {
        private readonly ICustomerAuthService _customerAuthService;

        public CustomerAuthController(ICustomerAuthService customerAuthService)
        {
            _customerAuthService = customerAuthService;
        }

        /// <summary>
        /// Đăng ký tài khoản customer
        /// POST: api/customer/auth/register
        /// Body: { "name": "Nguyen Van A", "email": "a@gmail.com", "password": "123456", "phone": "0901234567", "address": "..." }
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
        {
            var result = await _customerAuthService.RegisterAsync(
                request.Name,
                request.Email,
                request.Password,
                request.Phone,
                request.Address
            );

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.",
                customer = new
                {
                    result.Customer?.CustomerId,
                    result.Customer?.Name,
                    result.Customer?.Email
                }
            });
        }

        /// <summary>
        /// Đăng nhập customer
        /// POST: api/customer/auth/login
        /// Body: { "email": "a@gmail.com", "password": "123456" }
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCustomerRequest request)
        {
            var result = await _customerAuthService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                token = result.Token,
                customer = new
                {
                    result.Customer?.CustomerId,
                    result.Customer?.Name,
                    result.Customer?.Email,
                    result.Customer?.Phone,
                    result.Customer?.Address
                }
            });
        }

        /// <summary>
        /// Lấy thông tin customer đang đăng nhập
        /// GET: api/customer/auth/me
        /// Header: Authorization: Bearer {token}
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var customerIdClaim = User.FindFirst("customer_id")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new { message = "Token không hợp lệ" });

            var customer = await _customerAuthService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return NotFound(new { message = "Không tìm thấy thông tin customer" });

            return Ok(new
            {
                customerId = customer.CustomerId,
                name = customer.Name,
                email = customer.Email,
                phone = customer.Phone,
                address = customer.Address,
                createdAt = customer.CreatedAt
            });
        }

        /// <summary>
        /// Cập nhật thông tin customer
        /// PUT: api/customer/auth/profile
        /// Body: { "name": "New Name", "phone": "0909090909", "address": "New Address" }
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var customerIdClaim = User.FindFirst("customer_id")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new { message = "Token không hợp lệ" });

            var result = await _customerAuthService.UpdateCustomerProfileAsync(
                customerId,
                request.Name,
                request.Phone,
                request.Address
            );

            if (!result)
                return BadRequest(new { message = "Cập nhật thất bại" });

            return Ok(new { message = "Cập nhật thông tin thành công" });
        }

        /// <summary>
        /// Đổi mật khẩu
        /// POST: api/customer/auth/change-password
        /// Body: { "oldPassword": "123456", "newPassword": "654321" }
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var customerIdClaim = User.FindFirst("customer_id")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new { message = "Token không hợp lệ" });

            if (request.NewPassword.Length < 6)
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });

            var result = await _customerAuthService.ChangePasswordAsync(
                customerId,
                request.OldPassword,
                request.NewPassword
            );

            if (!result)
                return BadRequest(new { message = "Mật khẩu cũ không đúng" });

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }
    }

    // Request models
    public class RegisterCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class LoginCustomerRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
