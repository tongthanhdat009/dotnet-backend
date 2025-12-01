using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using System;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    /// <summary>
    /// API giỏ hàng cho customer (cần đăng nhập)
    /// </summary>
    [ApiController]
    [Route("api/customer/cart")]
    [Authorize] // Yêu cầu đăng nhập
    public class CustomerCartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CustomerCartController(ICartService cartService)
        {
            _cartService = cartService;
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
        /// Lấy giỏ hàng của mình
        /// GET: api/customer/cart
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            try
            {
                var customerId = GetCustomerId();
                var cart = await _cartService.GetCartAsync(customerId);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tất cả items trong giỏ hàng
        /// GET: api/customer/cart/items
        /// </summary>
        [HttpGet("items")]
        public async Task<IActionResult> GetMyCartItems()
        {
            try
            {
                var customerId = GetCustomerId();
                var items = await _cartService.GetCartItemsAsync(customerId);
                return Ok(items);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// POST: api/customer/cart/items
        /// Body: { "productId": 1, "quantity": 2 }
        /// </summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddCartItemRequest request)
        {
            try
            {
                var customerId = GetCustomerId();
                var cartItem = await _cartService.AddItemAsync(customerId, request.ProductId, request.Quantity);
                return Ok(new { message = "Đã thêm sản phẩm vào giỏ hàng", data = cartItem });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật số lượng của item
        /// PUT: api/customer/cart/items/{cartItemId}
        /// Body: { "quantity": 5 }
        /// </summary>
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, [FromBody] UpdateQuantityRequest request)
        {
            try
            {
                var cartItem = await _cartService.UpdateItemQuantityAsync(cartItemId, request.Quantity);
                if (cartItem == null)
                    return NotFound(new { message = "Không tìm thấy item trong giỏ hàng" });

                return Ok(new { message = "Đã cập nhật số lượng", data = cartItem });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa item khỏi giỏ hàng
        /// DELETE: api/customer/cart/items/{cartItemId}
        /// </summary>
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var result = await _cartService.RemoveItemAsync(cartItemId);
            if (!result)
                return NotFound(new { message = "Không tìm thấy item trong giỏ hàng" });

            return Ok(new { message = "Đã xóa item khỏi giỏ hàng" });
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// DELETE: api/customer/cart
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> ClearMyCart()
        {
            try
            {
                var customerId = GetCustomerId();
                var result = await _cartService.ClearCartAsync(customerId);
                if (!result)
                    return NotFound(new { message = "Không tìm thấy giỏ hàng" });

                return Ok(new { message = "Đã xóa toàn bộ giỏ hàng" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tính tổng giá trị giỏ hàng
        /// GET: api/customer/cart/total
        /// </summary>
        [HttpGet("total")]
        public async Task<IActionResult> GetMyCartTotal()
        {
            try
            {
                var customerId = GetCustomerId();
                var total = await _cartService.GetCartTotalAsync(customerId);
                return Ok(new { total });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
