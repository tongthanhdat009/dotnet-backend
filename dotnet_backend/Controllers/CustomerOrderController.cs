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
        private readonly IOrderItemService _orderItemService;
        private readonly ICartService _cartService;
        private readonly IPaymentService _paymentService;
        private readonly IInvoicePdfService _invoicePdfService;

        public CustomerOrderController(IOrderService orderService, IOrderItemService orderItemService, ICartService cartService, IPaymentService paymentService, IInvoicePdfService invoicePdfService)
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
            _cartService = cartService;
            _paymentService = paymentService;
            _invoicePdfService = invoicePdfService;
        }

        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("customer_id")?.Value;
            if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int customerId))
                throw new UnauthorizedAccessException("Token không hợp lệ");
            return customerId;
        }

        [HttpPost("create-from-cart")]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderFromCartRequest? request)
        {
            try
            {
                var customerId = GetCustomerId();
                var cartItems = await _cartService.GetCartItemsAsync(customerId);
                if (cartItems == null || cartItems.Count == 0)
                    return BadRequest(new { message = "Giỏ hàng trống" });

                var orderItems = new List<OrderItemDto>();
                foreach (var item in cartItems)
                {
                    if (item.ProductId <= 0) return BadRequest(new { message = $"ProductId không hợp lệ: {item.ProductId}" });
                    if (item.Quantity <= 0) return BadRequest(new { message = $"Số lượng không hợp lệ cho productId {item.ProductId}" });
                    if (item.Price < 0) return BadRequest(new { message = $"Price không hợp lệ cho productId {item.ProductId}" });

                    orderItems.Add(new OrderItemDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                var orderDto = new OrderDto
                {
                    CustomerId = customerId,
                    OrderItems = orderItems,
                    PromoId = request.PromoId
                };

                var order = await _orderService.CreateOrderAsync(orderDto);
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

        [HttpPost("preview")]
        public async Task<IActionResult> PreviewOrderFromCart([FromBody] CreateOrderFromCartRequest? request)
        {
            try
            {
                var customerId = GetCustomerId();
                request ??= new CreateOrderFromCartRequest();
                var order = await _orderService.PreviewOrderFromCartAsync(customerId, request.PromoId, request.PaymentMethod ?? "cash");
                return Ok(new { message = "Preview created", data = order });
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
        /// Lấy chi tiết đơn hàng
        /// GET: api/customer/orders/{orderId}/orderitem-with-product
        /// </summary>
        [HttpGet("{orderId}/orderitem-with-product")]
        public async Task<IActionResult> GetOrderItemWithProduct(int orderId)
        {
            try
            {
                var orderItemWithProducts = await _orderItemService.GetOrderItemsWithProductsAsync(orderId);
                if (orderItemWithProducts == null)
                    return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng" });
                return Ok(orderItemWithProducts);
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

        [HttpGet("online-orders-by-customer/{customerId}")]
        public async Task<IActionResult> GetOnlineOrdersByCustomerId(int customerId)
        {
            var onlineOrdersByCustomer = await _orderService.GetOnlineOrdersByCustomerIdAsync(customerId);
            return Ok(onlineOrdersByCustomer);
        }

        [HttpPost("update-order-and-bill-status")]
        public async Task<IActionResult> UpdateOrderAndBillStatus(
            [FromBody] UpdateOrderAndBillStatusDto request)
        {
            var result = await _orderService.UpdateOrderAndBillStatusAsync(
                request.OrderId,
                request.StatusOrder,
                request.StatusBill
            );

            return Ok(result);
        }
        /// <summary>
        /// Thanh toán toàn bộ giỏ hàng (tạo order và trả về orderDto)
        /// POST: api/customer/orders/checkout
        /// Body: { "promoCode": "SUMMER2024" } // optional
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto? request)
        {
            try
            {
                var customerId = GetCustomerId();

                // Gán customerId từ JWT token vào request
                request ??= new CheckoutDto();
                request.CustomerId = customerId;

                OrderDto order;
                // If frontend sent PromoId instead of PromoCode, use overload that accepts promoId
                if (request.PromoId.HasValue)
                {
                    order = await _orderService.CheckoutFromCartAsync(customerId, null, request.PromoId);
                }
                else
                {
                    order = await _orderService.CheckoutFromCartAsync(request);
                }

                return Ok(new { message = "Checkout thành công", data = order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Thanh toán cho order
        /// POST: api/customer/orders/{orderId}/pay
        /// Body: { "amount": 100000, "paymentMethod": "card" }
        /// </summary>
        [HttpPost("{orderId}/pay")]
        public async Task<IActionResult> PayOrder(int orderId, [FromBody] PaymentDto paymentDto)
        {
            Console.WriteLine($"[Controller] PayOrder API called: OrderId={orderId}, Amount={paymentDto.Amount}, Method={paymentDto.PaymentMethod}");

            try
            {
                var customerId = GetCustomerId();
                Console.WriteLine($"[Controller] CustomerId from JWT: {customerId}");

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    Console.WriteLine($"[Controller] Order {orderId} not found");
                    return NotFound(new { message = "Order không tồn tại" });
                }

                if (order.CustomerId != customerId)
                {
                    Console.WriteLine($"[Controller] Forbidden: Order {orderId} does not belong to customer {customerId}");
                    return Forbid();
                }

                var payment = await _paymentService.CreatePayment(paymentDto);

                Console.WriteLine($"[Controller] Payment successful: PaymentId={payment.PaymentId}, Amount={payment.Amount}");

                return Ok(new { message = "Thanh toán thành công", data = payment });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Controller] Exception: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tải hóa đơn điện tử dưới dạng PDF
        /// GET: api/customer/orders/{orderId}/invoice-pdf
        /// </summary>
        [HttpGet("{orderId}/invoice-pdf")]
        public async Task<IActionResult> DownloadInvoicePdf(int orderId)
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

                // Generate PDF
                var pdfBytes = await _invoicePdfService.GenerateInvoicePdfAsync(orderId);
                
                // Return PDF file
                return File(pdfBytes, "application/pdf", $"HoaDon_{orderId}_{DateTime.Now:yyyyMMdd}.pdf");
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

    }



    // Request model
    public class CreateOrderFromCartRequest
{
    public int? PromoId { get; set; }   
    public string? PaymentMethod { get; set; } = "cash"; 
}





}