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

    public string Status { get; set; } = "unpaid";

    public DateTime? CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Customer? Customer { get; set; }
}
