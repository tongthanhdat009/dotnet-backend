using dotnet_backend.Models;
using System.Threading.Tasks;

namespace dotnet_backend.Services.Interface
{
    public interface ICustomerAuthService
    {
        Task<(bool Success, string Message, string? Token, Customer? Customer)> RegisterAsync(string name, string email, string password, string? phone = null, string? address = null);
        Task<(bool Success, string Message, string? Token, Customer? Customer)> LoginAsync(string email, string password);
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<bool> UpdateCustomerProfileAsync(int customerId, string? name, string? phone, string? address);
        Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword);
    }
}
