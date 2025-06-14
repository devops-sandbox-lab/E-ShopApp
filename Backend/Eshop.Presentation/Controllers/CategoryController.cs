using Microsoft.AspNetCore.Mvc;
using Eshop.Application.Interfaces.Services;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var response = await _categoryService.GetAllCategoriesAsync();
            if (response.Succeeded)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        [HttpGet("subcategories")]
        public async Task<IActionResult> GetAllSubCategories()
        {
            var response = await _categoryService.GetAllSubCategoriesAsync();
            if (response.Succeeded)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryWithSubCategories(int id)
        {
            var response = await _categoryService.GetCategoryWithSubCategoriesAsync(id);
            if (response.Succeeded)
            {
                return Ok(response);
            }
            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }
            return StatusCode(500, response);
        }
    }
}