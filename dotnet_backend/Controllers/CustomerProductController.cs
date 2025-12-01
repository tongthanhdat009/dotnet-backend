using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_backend.Controllers
{
    /// <summary>
    /// API cho customer xem sản phẩm (không cần đăng nhập)
    /// </summary>
    [ApiController]
    [Route("api/customer/products")]
    [AllowAnonymous]
    public class CustomerProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public CustomerProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// Lấy tất cả sản phẩm
        /// GET: api/customer/products
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm
        /// GET: api/customer/products/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            return Ok(product);
        }

        /// <summary>
        /// Lấy sản phẩm theo category
        /// GET: api/customer/products/category/{categoryId}
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var allProducts = await _productService.GetAllProductsAsync();
            var products = allProducts.Where(p => p.CategoryId == categoryId).ToList();
            return Ok(products);
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo tên
        /// GET: api/customer/products/search?keyword=...
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "Keyword không được để trống" });

            var allProducts = await _productService.GetAllProductsAsync();
            var products = allProducts.Where(p => 
                p.ProductName != null && p.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            return Ok(products);
        }

        /// <summary>
        /// Lấy tất cả danh mục
        /// GET: api/customer/products/categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }
    }
}
