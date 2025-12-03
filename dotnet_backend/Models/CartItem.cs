using System;

namespace dotnet_backend.Models;

public partial class CartItem
{
    public int ProductId { get; set; }

    public int CustomerId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Subtotal { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
