using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace dotnet_backend.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _promotionService.GetAllPromotionsAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var promo = await _promotionService.GetPromotionByIdAsync(id);
            return Ok(promo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromotionDto dto)
    {
        try
        {
            var createdPromo = await _promotionService.CreatePromotionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPromo.PromoId }, createdPromo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PromotionDto dto)
    {
        try
        {
            var updatedPromo = await _promotionService.UpdatePromotionAsync(id, dto);
            return Ok(updatedPromo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _promotionService.DeletePromotionAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Không tìm thấy khuyến mãi với ID/Mã code này." });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyPromoRequestDto req)
    {
        try
        {
            var result = await _promotionService.ApplyPromotionAsync(req);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
