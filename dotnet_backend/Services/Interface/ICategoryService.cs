using System.Collections.Generic;
using System.Threading.Tasks;
using dotnet_backend.Dtos;

namespace dotnet_backend.Services.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<int> GetTotalCategoriesAsync();
        Task<CategoryDto> AddCategoryAsync(CategoryDto dto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, CategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
