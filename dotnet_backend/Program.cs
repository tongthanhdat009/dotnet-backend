using Microsoft.EntityFrameworkCore;
using dotnet_backend.Database;
using dotnet_backend.Services;
using dotnet_backend.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. Đăng ký dịch vụ
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ✅ 4. Bật CORS cho phép Vue (localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy
            .WithOrigins("http://localhost:5173") // địa chỉ Vue chạy
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// ✅ 5. Kích hoạt CORS
app.UseCors("AllowVueApp");

app.UseAuthorization();
app.MapControllers();

app.Run();
