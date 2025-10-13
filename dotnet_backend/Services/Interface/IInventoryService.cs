using dotnet_backend.Services;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface;

public interface IInventoryService
{
    Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
    Task<InventoryDto?> GetInventoryByIdAsync(int inventoryId);
    Task<InventoryDto?> GetInventoryByProductIdAsync(int productId);
    Task<InventoryDto?> UpdateInventoryAsync(int inventoryId, InventoryDto inventoryDto);
    Task<bool> UpdateQuantityAsync(int productId, int quantity);
}
