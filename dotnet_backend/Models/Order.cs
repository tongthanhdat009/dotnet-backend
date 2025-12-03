using System;
using System.Collections.Generic;

namespace dotnet_backend.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public int? UserId { get; set; }

    public int? PromoId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? OrderType { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Promotion? Promo { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
