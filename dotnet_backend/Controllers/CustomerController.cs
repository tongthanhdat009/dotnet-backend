using Microsoft.AspNetCore.Mvc;
using dotnet_backend.Services.Interface;
using dotnet_backend.Dtos;

namespace dotnet_backend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng" });
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto customerDto)
    {
        try
        {
            CustomerDto createdCustomer = await _customerService.CreateCustomerAsync(customerDto);
            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.CustomerId }, createdCustomer);
        }
        catch (ArgumentException ex) // Bắt lỗi nghiệp vụ từ Service
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDto customerDto)
    {
        try
        {
            CustomerDto updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerDto);
            return Ok(updatedCustomer);
        }
        catch (ArgumentException ex) // Bắt lỗi nghiệp vụ từ Service
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        bool result = await _customerService.DeleteCustomerAsync(id);
        if (!result) return NotFound(new { message = "Không tìm thấy khách hàng" });
        return NoContent();
    }
}
