using dotnet_backend.Services;
using dotnet_backend.Dtos;
namespace dotnet_backend.Services.Interface;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto);
    Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto customerDto);
    Task<bool> DeleteCustomerAsync(int id);
}
