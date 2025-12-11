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
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(10, 4, 0)) // MariaDB 10.4
    );
});


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
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // dễ debug
    });

// 5. Đăng ký các services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
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
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<PromotionService>();
builder.Services.AddScoped<IOrderService, OrderService>();


// ✅ 6. Bật CORS cho phép Vue (localhost:5173), Blazor (localhost:5000, localhost:5001, localhost:5192)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy
            .WithOrigins(
                "http://localhost:5173",  // Vue app
                "https://localhost:5001", // Blazor HTTPS
                "http://localhost:5000",  // Blazor HTTP
                "http://localhost:5192"   // Blazor HTTP (port thực tế)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Cho phép gửi cookie
});

var app = builder.Build();

// ✅ Tự động tạo database và các bảng khi khởi động lần đầu
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Đảm bảo database được tạo nếu chưa tồn tại
        dbContext.Database.EnsureCreated();
        Console.WriteLine("✅ Database đã được tạo hoặc đã tồn tại.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Lỗi khi tạo database: {ex.Message}");
    }
}

// ✅ 7. Kích hoạt CORS
app.UseCors("AllowVueApp");

// ✅ 8. 🔐 Kích hoạt Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
