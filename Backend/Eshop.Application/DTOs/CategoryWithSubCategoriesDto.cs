namespace Eshop.Application.DTOs
{
    public class CategoryWithSubCategoriesDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public List<SubCategoryDto> SubCategories { get; set; }
    }
}
