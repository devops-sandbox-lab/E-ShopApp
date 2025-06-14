using AutoMapper;
using Eshop.Application.DTOs;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Services;
using Eshop.Application.Interfaces.UnitOfWork;

namespace Eshop.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GeneralResponse<List<CategoryDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.categoryRepository.FindAllAsync();
                var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
                return new GeneralResponse<List<CategoryDto>>
                {
                    Data = categoryDtos,
                    Succeeded = true,
                    Message = "Categories retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<CategoryDto>>
                {
                    Data = null,
                    Succeeded = false,
                    Message = $"An error occurred while retrieving categories: {ex.Message}",
                    Errors = new List<string> { ex.Message }

                };
            }
        }

        public async Task<GeneralResponse<CategoryWithSubCategoriesDto>> GetCategoryWithSubCategoriesAsync(int categoryId)
        {
            try
            {
                var category = await _unitOfWork.categoryRepository.FindAllAsync(new[] { "SubCategories" }, c => c.CategoryId == categoryId);
                var categoryDto = _mapper.Map<CategoryWithSubCategoriesDto>(category.FirstOrDefault());
                return new GeneralResponse<CategoryWithSubCategoriesDto>
                {
                    Data = categoryDto,
                    Succeeded = categoryDto != null,
                    Message = categoryDto != null ? "Category with subcategories retrieved successfully." : "Category not found."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<CategoryWithSubCategoriesDto>
                {
                    Data = null,
                    Succeeded = false,
                    Message = $"An error occurred while retrieving the category with subcategories: {ex.Message}",
                    Errors = new List<string>() { ex.Message }

                };
            }
        }

        public async Task<GeneralResponse<List<SubCategoryDto>>> GetAllSubCategoriesAsync()
        {
            try
            {
                var subCategories = await _unitOfWork.categoryRepository.FindAllAsync(new[] { "SubCategories" }, c => c.SubCategories != null);
                var subCategoryDtos = _mapper.Map<List<SubCategoryDto>>(subCategories.SelectMany(c => c.SubCategories));
                return new GeneralResponse<List<SubCategoryDto>>
                {
                    Data = subCategoryDtos,
                    Succeeded = true,
                    Message = "Subcategories retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<SubCategoryDto>>
                {
                    Data = null,
                    Succeeded = false,
                    Message = $"An error occurred while retrieving subcategories: {ex.Message}",
                    Errors = new List<string>() { ex.Message }

                };
            }
        }
    }
}
