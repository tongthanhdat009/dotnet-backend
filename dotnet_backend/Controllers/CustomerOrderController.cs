using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    /// <summary>
    /// API đơn hàng cho customer (cần đăng nhập)
    /// </summary>
    [ApiController]
    [Route("api/customer/orders")]
    [Authorize]
    public class CustomerOrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public CustomerOrderController(IOrderService orderService, ICartService cartService)
        {
            _orderService = orderService;
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
        /// Tạo đơn hàng từ giỏ hàng
        /// POST: api/customer/orders/create-from-cart
        /// Body: { "promoCode": "SUMMER2024" } // optional
        /// </summary>
        [HttpPost("create-from-cart")]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderFromCartRequest? request)
        {
            try
            {
                var customerId = GetCustomerId();
                
                // Lấy items từ giỏ hàng
                var cartItems = await _cartService.GetCartItemsAsync(customerId);
                if (cartItems == null || cartItems.Count == 0)
                    return BadRequest(new { message = "Giỏ hàng trống" });

                // Tạo order items
                var orderItems = new List<OrderItemDto>();
                foreach (var item in cartItems)
                {
                    orderItems.Add(new OrderItemDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                // Tạo order
                var orderDto = new OrderDto
                {
                    CustomerId = customerId,
                    OrderItems = orderItems
                };

                var order = await _orderService.CreateOrderAsync(orderDto);

                // Xóa giỏ hàng sau khi đặt hàng thành công
                await _cartService.ClearCartAsync(customerId);

                return Ok(new { message = "Đặt hàng thành công", data = order });
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
        /// Lấy danh sách đơn hàng của mình
        /// GET: api/customer/orders
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var customerId = GetCustomerId();
                var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng
        /// GET: api/customer/orders/{orderId}
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            try
            {
                var customerId = GetCustomerId();
                var order = await _orderService.GetOrderByIdAsync(orderId);
                
                if (order == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });

                // Kiểm tra xem order có thuộc về customer này không
                if (order.CustomerId != customerId)
                    return Forbid();

                return Ok(order);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Hủy đơn hàng (chỉ hủy được khi status = pending)
        /// POST: api/customer/orders/{orderId}/cancel
        /// </summary>
        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var customerId = GetCustomerId();
                var order = await _orderService.GetOrderByIdAsync(orderId);
                
                if (order == null)
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });

                // Kiểm tra xem order có thuộc về customer này không
                if (order.CustomerId != customerId)
                    return Forbid();

                // Kiểm tra trạng thái
                if (order.Status != "pending")
                    return BadRequest(new { message = "Chỉ có thể hủy đơn hàng đang chờ xử lý" });

                var result = await _orderService.CancelOrderAsync(orderId);
                if (!result)
                    return BadRequest(new { message = "Hủy đơn hàng thất bại" });

                return Ok(new { message = "Đã hủy đơn hàng" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo trạng thái
        /// GET: api/customer/orders/by-status/{status}
        /// </summary>
        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            try
            {
                var customerId = GetCustomerId();
                var allOrders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
                
                var filteredOrders = allOrders.Where(o => 
                    o.Status?.ToLower() == status.ToLower()).ToList();

                return Ok(filteredOrders);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }

    // Request model
    public class CreateOrderFromCartRequest
    {
        public string? PromoCode { get; set; }
    }
}
