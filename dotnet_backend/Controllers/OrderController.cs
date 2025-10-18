using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly ICustomerService _customerService;

    public OrderController(IOrderService orderService, IPaymentService paymentService, ICustomerService customerService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
        return Ok(order);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        try
        {
            var success = await _orderService.CancelOrderAsync(id);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Không thể hủy đơn hàng (quá hạn, không tồn tại hoặc đã bị hủy)."
                });
            }

            return Ok(new
            {
                message = "Đơn hàng đã được hủy thành công."
            });
        }
        catch (Exception ex)
        {
            // Trả về lỗi chi tiết (debug mode)
            return StatusCode(500, new
            {
                message = "Đã xảy ra lỗi khi hủy đơn hàng.",
                error = ex.Message,
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderDto orderDto)
    {
        try
        {
            if (orderDto.CustomerId == null)
            {
                if (orderDto.Customer == null)
                {
                    return BadRequest(new
                    {
                        message = "Thông tin khách hàng không được để trống."
                    });
                }
                var customerDto = await _customerService.CreateCustomerAsync(orderDto.Customer);
                orderDto.CustomerId = customerDto.CustomerId;
            }

            OrderDto order = await _orderService.CreateOrderAsync(orderDto);
            foreach (var paymentDto in orderDto.Payments)
            {
                paymentDto.OrderId = order.OrderId;
                await _paymentService.CreatePayment(paymentDto);
            }

            var result = await _orderService.GetOrderByIdAsync(order.OrderId);
            return Ok(new
            {
                message = "Đơn hàng đã được tạo thành công.",
                Order = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Đã xảy ra lỗi khi tạo đơn hàng.",
                error = ex.Message,
            });
        }
    }
    
}
