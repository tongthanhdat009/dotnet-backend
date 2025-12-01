using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Lấy giỏ hàng của khách hàng
        /// GET: api/cart/{customerId}
        /// </summary>
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCart(int customerId)
        {
            var cart = await _cartService.GetCartAsync(customerId);
            if (cart == null)
                return NotFound(new { message = "Không tìm thấy giỏ hàng" });

            return Ok(cart);
        }

        /// <summary>
        /// Lấy tất cả items trong giỏ hàng
        /// GET: api/cart/{customerId}/items
        /// </summary>
        [HttpGet("{customerId}/items")]
        public async Task<IActionResult> GetCartItems(int customerId)
        {
            var items = await _cartService.GetCartItemsAsync(customerId);
            return Ok(items);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// POST: api/cart/{customerId}/items
        /// Body: { "productId": 1, "quantity": 2 }
        /// </summary>
        [HttpPost("{customerId}/items")]
        public async Task<IActionResult> AddItem(int customerId, [FromBody] AddCartItemRequest request)
        {
            try
            {
                var cartItem = await _cartService.AddItemAsync(customerId, request.ProductId, request.Quantity);
                return Ok(new { message = "Đã thêm sản phẩm vào giỏ hàng", data = cartItem });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật số lượng của item
        /// PUT: api/cart/items/{cartItemId}
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
        /// DELETE: api/cart/items/{cartItemId}
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
        /// DELETE: api/cart/{customerId}
        /// </summary>
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            var result = await _cartService.ClearCartAsync(customerId);
            if (!result)
                return NotFound(new { message = "Không tìm thấy giỏ hàng" });

            return Ok(new { message = "Đã xóa toàn bộ giỏ hàng" });
        }

        /// <summary>
        /// Tính tổng giá trị giỏ hàng
        /// GET: api/cart/{customerId}/total
        /// </summary>
        [HttpGet("{customerId}/total")]
        public async Task<IActionResult> GetCartTotal(int customerId)
        {
            var total = await _cartService.GetCartTotalAsync(customerId);
            return Ok(new { total });
        }
    }

    // Request models
    public class AddCartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateQuantityRequest
    {
        public int Quantity { get; set; }
    }
}
