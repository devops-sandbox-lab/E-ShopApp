using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eshop.Core.Entities
{
    public class Feature
    {
        [Key]
        public int FeatureId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FeatureName { get; set; }

        [MaxLength(255)]
        public string FeatureValue { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}

