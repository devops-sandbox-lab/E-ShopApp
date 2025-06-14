namespace Eshop.Core.Entities
{
    public class Customer : ApplicationUser
    {
        public string? ProfileImage { get; set; } = "/Images/Default-Customer-Img.svg";
        public string? ShippingAddress { get; set; }
    }
}
