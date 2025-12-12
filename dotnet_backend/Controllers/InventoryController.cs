using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers;

[Authorize] // Bảo vệ toàn bộ controller
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// Lấy danh sách tất cả inventory
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllInventories()
    {
        try
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(inventories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    /// Lấy inventory theo ID
    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetInventoryById(int id)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound(new { message = "Không tìm thấy inventory" });
            }
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    /// Lấy inventory theo Product ID
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<InventoryDto>> GetInventoryByProductId(int productId)
    {
        try
        {
            var inventory = await _inventoryService.GetInventoryByProductIdAsync(productId);
            if (inventory == null)
            {
                return NotFound(new { message = "Không tìm thấy inventory cho product này" });
            }
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    /// Cập nhật inventory
    [HttpPut("{id}")]
    public async Task<ActionResult<InventoryDto>> UpdateInventory(int id, [FromBody] InventoryDto inventoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedInventory = await _inventoryService.UpdateInventoryAsync(id, inventoryDto);
            if (updatedInventory == null)
            {
                return NotFound(new { message = "Không tìm thấy inventory để cập nhật" });
            }

            return Ok(updatedInventory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    /// Cập nhật số lượng inventory theo Product ID
    [HttpPatch("product/{productId}/quantity")]
    public async Task<ActionResult> UpdateQuantity(int productId, [FromBody] int request)
    {
        try
        {
            if (request < 0)
            {
                return BadRequest(new { message = "Số lượng không được âm" });
            }

            var updated = await _inventoryService.UpdateQuantityAsync(productId, request);
            if (!updated)
            {
                return NotFound(new { message = "Không tìm thấy inventory cho product này" });
            }

            return Ok(new { message = "Cập nhật số lượng thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }

    /// Validate stock cho cart - Cho phép anonymous access
    [AllowAnonymous]
    [HttpPost("customer/validate-cart-stock")]
    public async Task<ActionResult<ValidateCartStockResponse>> ValidateCartStock([FromBody] ValidateCartStockRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _inventoryService.ValidateCartStockAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
        }
    }
}
