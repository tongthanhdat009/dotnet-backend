using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers;

[Authorize] // Bảo vệ toàn bộ controller
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

    [HttpGet("promotions")]
    public async Task<IActionResult> GetPromotions()
    {
        var promos = await _orderService.GetAllPromosAsync();
        return Ok(promos);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetOrdersByCustomer(int customerId)
    {
        var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
        return Ok(orders); 
    }
    [HttpGet("total")]
    public async Task<IActionResult> GetTotalOrders()
    {
        var totalOrders = await _orderService.GetTotalOrdersAsync();
        return Ok(totalOrders);
    }

    [HttpGet("peak-time")]
    public async Task<IActionResult> GetPeakTimeStats()
    {
        var result = await _orderService.GetPeakTimeStatsAsync();
        return Ok(result);
    }

    [HttpGet("orders-by-year/{year}")]
    public async Task<IActionResult> GetOrdersByYear(int year)
    {
        var ordersByYear = await _orderService.GetOrdersByYearAsync(year);
        return Ok(ordersByYear);
    }

    [HttpGet("sales-by-year/{year}")]
    public async Task<IActionResult> GetSalesByYear(int year)
    {
        var salesByYear = await _orderService.GetSalesByYearAsync(year);
        return Ok(salesByYear);
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
                message = ex.Message,
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(OrderDto orderDto)
    {
        try
        {
            OrderDto order = await _orderService.CreateOrderAsync(orderDto);
            if (orderDto.Payments != null)
            {
                foreach (var paymentDto in orderDto.Payments)
                {
                    paymentDto.OrderId = order.OrderId;
                    await _paymentService.CreatePayment(paymentDto);
                }
            }

            var result = await _orderService.GetOrderByIdAsync(order.OrderId);
            return Ok(new
            {
                message = "Đơn hàng đã được tạo thành công.",
                Order = result
            });
        }
        catch (ArgumentException aex)
        {
            return BadRequest(new
            {
                message = aex.Message
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