using dotnet_backend.Services;
using dotnet_backend.Dtos;
namespace dotnet_backend.Services.Interface;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto> GetOrderByIdAsync(int id);
    Task<int> GetTotalOrdersAsync();
    Task<IEnumerable<OrderByMonthDto>> GetOrdersByYearAsync(int year);
    Task<IEnumerable<SalesByMonthDto>> GetSalesByYearAsync(int year);
    Task<IEnumerable<PeakTimeDto>> GetPeakTimeStatsAsync();
    Task<bool> CancelOrderAsync(int orderId);
    Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
}