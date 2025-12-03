using dotnet_backend.Database;
using dotnet_backend.Models;
using dotnet_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_backend.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả items trong giỏ hàng của khách hàng
        /// </summary>
        public async Task<List<CartItem>> GetCartItemsAsync(int customerId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
                .Where(ci => ci.CustomerId == customerId)
                .ToListAsync();
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        public async Task<CartItem> AddItemAsync(int customerId, int productId, int quantity = 1)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

            // Lấy sản phẩm từ database
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new ArgumentException("Sản phẩm không tồn tại", nameof(productId));

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.ProductId == productId);

            if (existingItem != null)
            {
                // Nếu đã có, cập nhật số lượng
                existingItem.Quantity += quantity;
                existingItem.Subtotal = existingItem.Quantity * existingItem.Price;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                // Nếu chưa có, thêm mới
                var newItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price,
                    Subtotal = quantity * product.Price,
                    AddedAt = DateTime.Now
                };
                _context.CartItems.Add(newItem);
                existingItem = newItem;
            }

            await _context.SaveChangesAsync();
            return existingItem;
        }

        /// <summary>
        /// Cập nhật số lượng của một item trong giỏ hàng
        /// </summary>
        public async Task<CartItem?> UpdateItemQuantityAsync(int customerId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.ProductId == productId);

            if (cartItem == null)
                return null;

            cartItem.Quantity = quantity;
            cartItem.Subtotal = quantity * cartItem.Price;

            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            return cartItem;
        }

        /// <summary>
        /// Xóa một item khỏi giỏ hàng
        /// </summary>
        public async Task<bool> RemoveItemAsync(int customerId, int productId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.ProductId == productId);

            if (cartItem == null)
                return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng của khách hàng
        /// </summary>
        public async Task<bool> ClearCartAsync(int customerId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
                return false;

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tính tổng giá trị giỏ hàng
        /// </summary>
        public async Task<decimal> GetCartTotalAsync(int customerId)
        {
            var total = await _context.CartItems
                .Where(ci => ci.CustomerId == customerId)
                .SumAsync(ci => ci.Subtotal);

            return total;
        }

        /// <summary>
        /// Lấy cart item theo sản phẩm
        /// </summary>
        public async Task<CartItem?> GetCartItemByProductAsync(int customerId, int productId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.ProductId == productId);
        }
    }
}
