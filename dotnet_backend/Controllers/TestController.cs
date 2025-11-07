using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Database;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Controllers
{
    [AllowAnonymous] // Controller test không cần xác thực
    [ApiController]
    [Route("api/[controller]")] // URL sẽ là /api/test
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Khi truy cập GET /api/test
        [HttpGet]
        public IActionResult GetTestMessage()
        {
            // Trả về một đối tượng JSON đơn giản
            return Ok(new { message = "API đang hoạt động tốt!" });
        }

        // Kiểm tra kết nối MySQL - GET /api/test/mysql
        [HttpGet("mysql")]
        public async Task<IActionResult> TestMySqlConnection()
        {
            try
            {
                // Thử kết nối đến database
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Lấy thông tin version của MySQL
                    var connection = _context.Database.GetDbConnection();
                    await connection.OpenAsync();
                    var serverVersion = connection.ServerVersion;
                    await connection.CloseAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Kết nối MySQL thành công!",
                        serverVersion = serverVersion,
                        database = connection.Database,
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Không thể kết nối đến MySQL",
                        timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi kết nối MySQL",
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        // Kiểm tra chi tiết database - GET /api/test/mysql/info
        [HttpGet("mysql/info")]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                var result = new
                {
                    success = true,
                    database = connection.Database,
                    serverVersion = connection.ServerVersion,
                    state = connection.State.ToString(),
                    connectionString = connection.ConnectionString.Replace(connection.ConnectionString.Split("Password=")[1].Split(";")[0], "****"), // Ẩn password
                    timestamp = DateTime.Now
                };

                await connection.CloseAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin database",
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }
    }
}