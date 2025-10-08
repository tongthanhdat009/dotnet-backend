// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;

namespace dotnet_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    // Inject service vào controller
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products); // Trả về status 200 OK cùng với danh sách sản phẩm
    }

}