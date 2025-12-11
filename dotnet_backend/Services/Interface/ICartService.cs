using dotnet_backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet_backend.Services.Interface
{
    public interface ICartService
    {
        // Lấy tất cả items trong giỏ hàng
        Task<List<CartItem>> GetCartItemsAsync(int customerId);

        // Thêm sản phẩm vào giỏ hàng
        Task<CartItem> AddItemAsync(int customerId, int productId, int quantity = 1);

        // Cập nhật số lượng của item trong giỏ hàng
        Task<CartItem?> UpdateItemQuantityAsync(int customerId, int productId, int quantity);

        // Xóa một item khỏi giỏ hàng
        Task<bool> RemoveItemAsync(int customerId, int productId);

        // Xóa toàn bộ giỏ hàng
        Task<bool> ClearCartAsync(int customerId);

        // Tính tổng giá trị giỏ hàng
        Task<decimal> GetCartTotalAsync(int customerId);

        // Kiểm tra xem sản phẩm có trong giỏ hàng không
        Task<CartItem?> GetCartItemByProductAsync(int customerId, int productId);
    }
}