using dotnet_backend.Services;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IOrderItemService
{
    Task<IEnumerable<OrderItemWithProductDto>> GetOrderItemsWithProductsAsync(int orderId);
}