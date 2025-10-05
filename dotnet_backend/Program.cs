using Microsoft.EntityFrameworkCore;
// Thêm các using khác nếu cần, ví dụ: using MyProject.Data;
using dotnet_backend.Database;

var builder = WebApplication.CreateBuilder(args);

// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký DbContext và cấu hình để sử dụng MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// Thêm các dịch vụ khác...
builder.Services.AddControllers();

var app = builder.Build();

// Cấu hình routing cho controllers
app.MapControllers();

app.Run();