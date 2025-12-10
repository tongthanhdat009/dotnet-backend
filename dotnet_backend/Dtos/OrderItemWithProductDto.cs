using System.Collections.Generic;
using dotnet_backend.Models;

namespace dotnet_backend.Dtos
{
    public class OrderItemWithProductDto
    {
        public int OrderItemId { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }

        // Thông tin từ bảng products
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public decimal ProductPrice { get; set; }
        public string Unit { get; set; }
    }
}
