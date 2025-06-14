using Eshop.Application.DTOs;
using Eshop.Application.GeneralResponse;

namespace Eshop.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<GeneralResponse<List<CategoryDto>>> GetAllCategoriesAsync();
        Task<GeneralResponse<CategoryWithSubCategoriesDto>> GetCategoryWithSubCategoriesAsync(int categoryId);
        Task<GeneralResponse<List<SubCategoryDto>>> GetAllSubCategoriesAsync();

        //public Task<GeneralResponse<string>> GetCategoryNameById(int catId);
    }
}
