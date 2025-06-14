using Microsoft.AspNetCore.Http;

namespace Eshop.Application.DTOs
{
    public class SellerRegisterDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string SecurityQuestion { get; set; }
        public bool HasOwnShipping { get; set; }
        public string ShopName { get; set; }

        public string TaxRegisterNumber { get; set; }
        public DateTime TaxRegisterExpireDate { get; set; }
        public string? TaxRegisterPDF { get; set; }
        public IFormFile TaxRegisterPDFFile { get; set; }
    }
}
