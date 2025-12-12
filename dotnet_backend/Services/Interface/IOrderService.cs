using dotnet_backend.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace dotnet_backend.Services.Interface;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrdersOfflineAsync();
    Task<IEnumerable<OrderDto>> GetOrdersOnlineAsync();
    Task<OrderDto> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
    Task<int> GetTotalOrdersAsync();
    Task<int> UpdateOrderAndBillStatusAsync(int orderId, string statusOrder, string statusBill);
    Task<IEnumerable<OrderByMonthDto>> GetOrdersByYearAsync(int year);
    Task<IEnumerable<SalesByMonthDto>> GetSalesByYearAsync(int year);
    Task<IEnumerable<OrderDto>> GetOnlineOrdersByCustomerIdAsync(int customerId);
    Task<IEnumerable<PeakTimeDto>> GetPeakTimeStatsAsync();
    Task<bool> CancelOrderAsync(int orderId);
    Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
    Task<IEnumerable<PromotionDto>> GetAllPromosAsync();
    Task<OrderDto> CheckoutFromCartAsync(Dtos.CheckoutDto checkout);
    Task<OrderDto> CheckoutFromCartAsync(int customerId, int? userId = null, int? promoId = null);
    Task<OrderDto> PreviewOrderFromCartAsync(int customerId, int? promoId = null, string paymentMethod = "cash");
}
