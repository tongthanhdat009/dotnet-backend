namespace dotnet_backend.Dtos;

public class PaymentDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
    public virtual OrderDto? Order { get; set; }
}