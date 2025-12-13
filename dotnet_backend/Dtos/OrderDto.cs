namespace dotnet_backend.Dtos;

public class OrderDto
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public int? UserId { get; set; }

    public int? PromoId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? PayStatus { get; set; }

    public string? OrderStatus { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? OrderType { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual CustomerDto? Customer { get; set; }

    public virtual ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

    public virtual ICollection<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

    public virtual PromotionDto? Promo { get; set; }

    public virtual UserDto? User { get; set; }
}