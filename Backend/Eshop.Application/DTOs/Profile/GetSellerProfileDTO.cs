namespace Eshop.Application.DTOs.Profile
{
    public class GetSellerProfileDTO
    {
        public string SellerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImage { get; set; }
        public string Address { get; set; }
        public string SecurityQuestion { get; set; }
        public bool HasOwnShipping { get; set; }
        public string? AccountStatus { get; set; }

        public string ShopName { get; set; }
        public bool IsBlocked { get; set; }

        public string TaxRegisterNumber { get; set; }
        public DateTime TaxRegisterExpireDate { get; set; }
        public string? TaxRegisterPDF { get; set; }

        public DateTime DateCreated { get; set; }

    }
}
