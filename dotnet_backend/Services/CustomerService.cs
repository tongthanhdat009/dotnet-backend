using dotnet_backend.Services.Interface;
using dotnet_backend.Models;
using dotnet_backend.Database;
using dotnet_backend.Dtos;
using Microsoft.EntityFrameworkCore;
namespace dotnet_backend.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    // Dùng Dependency Injection để inject DbContext vào
    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
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

    public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersByOrderCountAsync(int topCount = 3)
    {
        var result = await _context.Orders
            .GroupBy(o => o.Customer.Name)
            .Select(g => new TopCustomerDto
            {
                Name = g.Key,
                TotalOrders = g.Count()
            })
            .OrderByDescending(u => u.TotalOrders)
            .Take(topCount)
            .ToListAsync();

        return result;
    }


    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto)
    {
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
        if (customer == null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
}
