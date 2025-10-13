using System.Collections.Generic;
using dotnet_backend.Models; // 👈 để nhận ra class Product

namespace dotnet_backend.Dtos
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Nếu bạn chưa cần danh sách sản phẩm, có thể comment dòng dưới lại
        public List<Product>? Products { get; set; }
    }
}
