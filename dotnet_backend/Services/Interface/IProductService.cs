using dotnet_backend.Services;

namespace dotnet_backend.Services.Interface;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> GetProductByIdAsync(int id);
}
