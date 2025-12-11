using dotnet_backend.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_backend.Services.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
        Task<int> GetTotalOrdersAsync();
        Task<IEnumerable<OrderByMonthDto>> GetOrdersByYearAsync(int year);
        Task<IEnumerable<SalesByMonthDto>> GetSalesByYearAsync(int year);
        Task<IEnumerable<PeakTimeDto>> GetPeakTimeStatsAsync();
        Task<bool> CancelOrderAsync(int orderId);
        Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
        Task<IEnumerable<PromotionDto>> GetAllPromosAsync();

        // Checkout
        Task<OrderDto> CheckoutFromCartAsync(Dtos.CheckoutDto checkout);
        Task<OrderDto> CheckoutFromCartAsync(int customerId, int? userId = null, int? promoId = null);
        // Build an OrderDto from cart without persisting or modifying the cart/inventory
        Task<OrderDto> PreviewOrderFromCartAsync(int customerId, int? promoId = null, string paymentMethod = "cash");
    }
}
