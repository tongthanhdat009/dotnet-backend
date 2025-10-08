using Microsoft.EntityFrameworkCore;
using dotnet_backend.Database;
using dotnet_backend.Services;
using dotnet_backend.Services.Interface;

// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Đảm bảo tên "DefaultConnection" khớp với trong file appsettings.json

// 2. Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 3. Thêm dịch vụ vào container (DI)
builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>(); // Quan trọng!

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();