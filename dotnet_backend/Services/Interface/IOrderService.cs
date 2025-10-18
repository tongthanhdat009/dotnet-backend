using dotnet_backend.Services;
using dotnet_backend.Dtos;
namespace dotnet_backend.Services.Interface;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto> GetOrderByIdAsync(int id);
    Task<bool> CancelOrderAsync(int orderId);
    Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
}