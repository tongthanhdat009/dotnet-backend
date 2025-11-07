using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dotnet_backend.Database;
using dotnet_backend.Services;
using dotnet_backend.Services.Interface;
using dotnet_backend.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký DbContext với timeout 600 giây
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString, 
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.CommandTimeout(600) // Timeout 10 phút
    ));

// 3. 🔐 Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"];
var key = Encoding.ASCII.GetBytes(secretKey ?? "");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Đăng ký dịch vụ
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 5. Đăng ký các services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();

// ✅ 6. Bật CORS cho phép Vue (localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy
            .WithOrigins("http://localhost:5173") // địa chỉ Vue chạy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Cho phép gửi cookie
});

var app = builder.Build();

// ✅ 7. Kích hoạt CORS
app.UseCors("AllowVueApp");

// ✅ 8. 🔐 Kích hoạt Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
