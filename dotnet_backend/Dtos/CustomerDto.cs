namespace dotnet_backend.Dtos;

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderDto> Orders { get; set; } = new List<OrderDto>();
}
