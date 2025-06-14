namespace Eshop.Application.DTOs.Product
{
    public class EditFeatureDTO
    {
        public int FeatureId { get; set; }
        public int ProductId { get; set; }
        public string FeatureName { get; set; }
        public string FeatureValue { get; set; }
    }
}
