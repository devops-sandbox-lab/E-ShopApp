namespace Eshop.Application.Helpers
{
    public static class ProductSizeHelper
    {
        public static bool RequiresSizes(int categoryId)
        {
            return categoryId <= 5;
        }
    }
}
