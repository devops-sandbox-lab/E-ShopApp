using Eshop.Core.Enums;

namespace Eshop.Core.Entities
{
    public class Seller : ApplicationUser
    {
        public string? ProfileImage { get; set; } = "/Images/Default-Seller-Img.svg";
        public bool HasOwnShipping { get; set; }
        public SellerAccountStatus AccountStatus { get; set; } = SellerAccountStatus.Pending;
        public bool IsAdminSeen { get; set; }

        public string ShopName { get; set; }

        public string TaxRegisterNumber { get; set; }
        public DateTime TaxRegisterExpireDate { get; set; }
        public string TaxRegisterPDF { get; set; }

    }
}
