using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Eshop.Application.DTOs
{
    public class EmailDTO
    {
        [Required]
        public string To { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public IList<IFormFile>? Attatchments { get; set; }
    }
}
