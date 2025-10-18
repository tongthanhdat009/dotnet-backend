namespace dotnet_backend.Dtos;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
public class CustomerDto : IValidatableObject
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderDto> Orders { get; set; } = new List<OrderDto>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // 1. Kiểm tra Name (bắt buộc)
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Tên khách hàng là bắt buộc.", new[] { nameof(Name) });
        }
        else if (Name.Length > 100)
        {
            yield return new ValidationResult("Tên không được vượt quá 100 ký tự.", new[] { nameof(Name) });
        }

        // 2. Kiểm tra Phone (chỉ khi có giá trị)
        if (!string.IsNullOrWhiteSpace(Phone))
        {
            var phoneRegex = @"^(0(3|5|7|8|9))[0-9]{8}$";
            if (!Regex.IsMatch(Phone, phoneRegex))
            {
                yield return new ValidationResult("Số điện thoại không đúng định dạng Việt Nam.", new[] { nameof(Phone) });
            }
        }

        // 3. Kiểm tra Email (chỉ khi có giá trị)
        if (!string.IsNullOrWhiteSpace(Email))
        {
            if (Email.Length >= 100)
            {
                yield return new ValidationResult("Email phải ít hơn 100 ký tự.", new[] { nameof(Email) });
            }
            // Dùng lại attribute có sẵn để kiểm tra định dạng
            if (!new EmailAddressAttribute().IsValid(Email))
            {
                yield return new ValidationResult("Email không đúng định dạng.", new[] { nameof(Email) });
            }
        }
    }
}
