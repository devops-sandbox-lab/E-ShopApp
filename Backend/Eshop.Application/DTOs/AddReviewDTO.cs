using Microsoft.AspNetCore.Http;

namespace Eshop.Application.DTOs
{
    public class AddReviewDTO
    {

        public int ProductId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public string? customerName { get; set; }
        public string? customerImage { get; set; }
        public IFormFile? ReviewImage { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}
