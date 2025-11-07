using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers
{
    [Authorize] // Bảo vệ toàn bộ controller
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        // 🔹 Lấy danh sách nhà cung cấp
        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return Ok(suppliers);
        }

        // 🔹 Lấy 1 nhà cung cấp theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null) return NotFound();
            return Ok(supplier);
        }

        // 🔹 Thêm nhà cung cấp mới
        [HttpPost]
        public async Task<IActionResult> AddSupplier([FromBody] SupplierDto dto)
        {
            var newSupplier = await _supplierService.AddSupplierAsync(dto);
            return CreatedAtAction(nameof(GetSupplier), new { id = newSupplier.SupplierId }, newSupplier);
        }

        // 🔹 Cập nhật nhà cung cấp
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierDto dto)
        {
            var updated = await _supplierService.UpdateSupplierAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // 🔹 Xóa nhà cung cấp
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var deleted = await _supplierService.DeleteSupplierAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
