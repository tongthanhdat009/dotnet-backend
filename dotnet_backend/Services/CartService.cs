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
        /// Lấy giỏ hàng của khách hàng, tự động tạo mới nếu chưa có
        /// </summary>
        public async Task<Cart?> GetCartAsync(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            // Tự động tạo giỏ hàng mới nếu chưa tồn tại
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        /// <summary>
        /// Lấy tất cả items trong giỏ hàng của khách hàng
        /// </summary>
        public async Task<List<CartItem>> GetCartItemsAsync(int customerId)
        {
            var cart = await GetCartAsync(customerId);
            if (cart == null) return new List<CartItem>();

            return await _context.CartItems
                .Include(ci => ci.Product)
                .ThenInclude(p => p.Category)
                .Where(ci => ci.CartId == cart.CartId)
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

            // Lấy hoặc tạo giỏ hàng
            var cart = await GetCartAsync(customerId);
            if (cart == null)
                throw new Exception("Không thể tạo giỏ hàng");

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == product.ProductId);

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
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Quantity = quantity,
                    Price = product.Price,
                    Subtotal = quantity * product.Price,
                    AddedAt = DateTime.Now
                };
                _context.CartItems.Add(newItem);
                existingItem = newItem;
            }

            // Cập nhật thời gian của giỏ hàng
            cart.UpdatedAt = DateTime.Now;
            _context.Carts.Update(cart);

            await _context.SaveChangesAsync();
            return existingItem;
        }

        /// <summary>
        /// Cập nhật số lượng của một item trong giỏ hàng
        /// </summary>
        public async Task<CartItem?> UpdateItemQuantityAsync(int cartItemId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null)
                return null;

            cartItem.Quantity = quantity;
            cartItem.Subtotal = quantity * cartItem.Price;

            // Cập nhật thời gian giỏ hàng
            cartItem.Cart.UpdatedAt = DateTime.Now;

            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            return cartItem;
        }

        /// <summary>
        /// Xóa một item khỏi giỏ hàng
        /// </summary>
        public async Task<bool> RemoveItemAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null)
                return false;

            // Cập nhật thời gian giỏ hàng
            cartItem.Cart.UpdatedAt = DateTime.Now;
            _context.Carts.Update(cartItem.Cart);

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng của khách hàng
        /// </summary>
        public async Task<bool> ClearCartAsync(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
                return false;

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.Now;
            _context.Carts.Update(cart);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tính tổng giá trị giỏ hàng
        /// </summary>
        public async Task<decimal> GetCartTotalAsync(int customerId)
        {
            var cart = await GetCartAsync(customerId);
            if (cart == null) return 0;

            var total = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .SumAsync(ci => ci.Subtotal);

            return total;
        }

        /// <summary>
        /// Lấy cart item theo sản phẩm
        /// </summary>
        public async Task<CartItem?> GetCartItemByProductAsync(int customerId, int productId)
        {
            var cart = await GetCartAsync(customerId);
            if (cart == null) return null;

            return await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);
        }
    }
}
