using dotnet_backend.Database;
using dotnet_backend.Models;
using dotnet_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_backend.Services
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CustomerAuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Đăng ký customer mới
        /// </summary>
        public async Task<(bool Success, string Message, string? Token, Customer? Customer)> RegisterAsync(
            string name, string email, string password, string? phone = null, string? address = null)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Tên không được để trống", null, null);

            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email không được để trống", null, null);

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Mật khẩu không được để trống", null, null);

            if (password.Length < 6)
                return (false, "Mật khẩu phải có ít nhất 6 ký tự", null, null);

            // Kiểm tra email đã tồn tại
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);

            if (existingCustomer != null)
                return (false, "Email đã được đăng ký", null, null);

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Tạo customer mới
            var customer = new Customer
            {
                Name = name,
                Email = email,
                Password = hashedPassword,
                Phone = phone,
                Address = address,
                CreatedAt = DateTime.Now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Tạo JWT token
            var token = GenerateJwtToken(customer);

            return (true, "Đăng ký thành công", token, customer);
        }

        /// <summary>
        /// Đăng nhập customer
        /// </summary>
        public async Task<(bool Success, string Message, string? Token, Customer? Customer)> LoginAsync(
            string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email không được để trống", null, null);

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Mật khẩu không được để trống", null, null);

            // Tìm customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
                return (false, "Email hoặc mật khẩu không đúng", null, null);

            if (string.IsNullOrEmpty(customer.Password))
                return (false, "Tài khoản chưa thiết lập mật khẩu", null, null);

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, customer.Password);

            if (!isPasswordValid)
                return (false, "Email hoặc mật khẩu không đúng", null, null);

            // Tạo JWT token
            var token = GenerateJwtToken(customer);

            return (true, "Đăng nhập thành công", token, customer);
        }

        /// <summary>
        /// Lấy thông tin customer theo ID
        /// </summary>
        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        /// <summary>
        /// Lấy thông tin customer theo Email
        /// </summary>
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        /// <summary>
        /// Cập nhật thông tin customer
        /// </summary>
        public async Task<bool> UpdateCustomerProfileAsync(int customerId, string? name, string? phone, string? address)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return false;

            if (!string.IsNullOrWhiteSpace(name))
                customer.Name = name;

            customer.Phone = phone;
            customer.Address = address;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int customerId, string oldPassword, string newPassword)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null || string.IsNullOrEmpty(customer.Password))
                return false;

            // Verify old password
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, customer.Password))
                return false;

            if (newPassword.Length < 6)
                return false;

            // Hash new password
            customer.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Tạo JWT token cho customer
        /// </summary>
        private string GenerateJwtToken(Customer customer)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Secret"];
            var key = Encoding.ASCII.GetBytes(secretKey ?? "");

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                    new Claim(ClaimTypes.Email, customer.Email ?? ""),
                    new Claim(ClaimTypes.Name, customer.Name),
                    new Claim("customer_id", customer.CustomerId.ToString()),
                    new Claim("role", "customer") // Phân biệt với user (staff/admin)
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token hết hạn sau 7 ngày
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
