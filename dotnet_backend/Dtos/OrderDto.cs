namespace dotnet_backend.Dtos;

public class OrderDto
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public int? UserId { get; set; }

    public int? PromoId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public virtual CustomerDto? Customer { get; set; }

    public virtual ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

    public virtual ICollection<PaymentDto> Payments { get; set; } = new List<PaymentDto>();

    public virtual PromotionDto? Promo { get; set; }

    public virtual UserDto? User { get; set; }
}