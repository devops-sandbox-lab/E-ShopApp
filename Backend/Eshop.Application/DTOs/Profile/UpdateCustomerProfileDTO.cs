using Microsoft.AspNetCore.Http;

namespace Eshop.Application.DTOs.Profile
{
    public class UpdateCustomerProfileDTO
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? imgURL { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string SecurityQuestion { get; set; }

        public string ShippingAddress { get; set; }
    }
}
