namespace Eshop.Application.DTOs.Profile
{
    public class GetCustomerProfileDTO
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string SecurityQuestion { get; set; }
        public string ProfileImage { get; set; }
        public string ShippingAddress { get; set; }
    }
}
