using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using dotnet_backend.Database;
using dotnet_backend.Dtos;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace dotnet_backend.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    // Dùng Dependency Injection để inject DbContext vào
    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Centralized validation previously in DTO moved here
    // Note: only validate syntax/business rules that don't require DB access.
    private async Task ValidateCustomerDto(CustomerDto dto, int? excludeCustomerId = null)
    {
        var errors = new List<string>();

        // Name: required, <= 100
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Tên khách hàng là bắt buộc.");
        else if (dto.Name.Length > 100)
            errors.Add("Tên không được vượt quá 100 ký tự.");

        // Phone: required and VN format
        if (string.IsNullOrWhiteSpace(dto.Phone))
        {
            errors.Add("Số điện thoại là bắt buộc.");
        }
        else
        {
            var phoneRegex = new Regex(@"^(0(3|5|7|8|9))[0-9]{8}$");
            if (!phoneRegex.IsMatch(dto.Phone))
                errors.Add("Số điện thoại không đúng định dạng Việt Nam.");
        }

        // Email: optional, < 100 and valid format
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            if (dto.Email.Length >= 100)
                errors.Add("Email phải ít hơn 100 ký tự.");
            else if (!new EmailAddressAttribute().IsValid(dto.Email))
                errors.Add("Email không đúng định dạng.");
        }

        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors));
    }
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        return await _context.Customers.Select(c => new CustomerDto
        {
            CustomerId = c.CustomerId,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address
        }).ToListAsync();
    }
    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return null!;
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address
        };
    }
    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto)
    {
        // Validate input
        await ValidateCustomerDto(customerDto, null);

        // Phone must be unique across all customers
        if (!string.IsNullOrWhiteSpace(customerDto.Phone))
        {
            var phoneExists = await _context.Customers.AnyAsync(c => c.Phone == customerDto.Phone);
            if (phoneExists)
                throw new ArgumentException("Số điện thoại đã tồn tại.");
        }

        var customer = new Customer
        {
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            Address = customerDto.Address,
            CreatedAt = DateTime.UtcNow
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt
        };
    }
    public async Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto customerDto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) throw new ArgumentException("Không tìm thấy khách hàng");

        // Validate input (exclude current customer id when checking uniqueness)
        await ValidateCustomerDto(customerDto, id);

        // Phone must be unique across all customers except current record
        if (!string.IsNullOrWhiteSpace(customerDto.Phone))
        {
            var phoneExists = await _context.Customers.AnyAsync(c => c.Phone == customerDto.Phone && c.CustomerId != id);
            if (phoneExists)
                throw new ArgumentException("Số điện thoại đã tồn tại.");
        }

        customer.Name = customerDto.Name;
        customer.Email = customerDto.Email;
        customer.Phone = customerDto.Phone;
        customer.Address = customerDto.Address;

        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt
        };
    }
    public async Task<bool> DeleteCustomerAsync(int id)
    {
    var customer = await _context.Customers.FindAsync(id);
    if (customer == null) throw new ArgumentException("Không tìm thấy khách hàng");

        // Check if there are any orders for this customer
        var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id);
        if (hasOrders)
        {
            // Do not allow deletion if customer has orders
            throw new ArgumentException("Không thể xóa: khách hàng có hóa đơn.");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
}
