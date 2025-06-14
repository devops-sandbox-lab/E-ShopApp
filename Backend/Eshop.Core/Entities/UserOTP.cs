namespace Eshop.Core.Entities
{
    public class UserOTP
    {
        public int Id { get; set; }

        public string OTP { get; set; }

        public string Email { get; set; }

        public DateTime OTPGeneratedTime { get; set; }
    }
}
