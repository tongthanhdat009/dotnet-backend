using System;

namespace dotnet_backend.Dtos
{
    public class BillDto
    {
        public int BillId { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class CreateBillDto
    {
        public int OrderId { get; set; }
    }
}
