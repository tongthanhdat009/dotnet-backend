using System;

namespace dotnet_backend.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? PaymentMethod { get; set; }

    public string PayStatus { get; set; } = "unpaid";

    public string BillStatus { get; set; } = "pending";

    public DateTime? CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Customer? Customer { get; set; }
}
