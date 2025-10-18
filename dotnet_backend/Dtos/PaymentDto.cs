namespace dotnet_backend.Dtos;
using System.ComponentModel.DataAnnotations;
public class PaymentDto : IValidatableObject
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
    public virtual OrderDto? Order { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // 2. Kiểm tra PaymentMethod (bắt buộc)
        var validMethods = new[] { "e-wallet", "cash" };
        if (string.IsNullOrWhiteSpace(PaymentMethod))
        {
            yield return new ValidationResult("Phương thức thanh toán là bắt buộc.", new[] { nameof(PaymentMethod) });
        }
        else if (!validMethods.Contains(PaymentMethod))
        {
            yield return new ValidationResult($"Phương thức thanh toán không hợp lệ. Các phương thức hợp lệ: {string.Join(", ", validMethods)}.", new[] { nameof(PaymentMethod) });
        }
    }
}