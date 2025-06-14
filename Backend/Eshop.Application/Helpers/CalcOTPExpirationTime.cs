namespace Eshop.Application.Helpers
{
    public static class CalcOTPExpirationTime
    {
        public static bool IsOTPExpired(DateTime otpGenerationTime, int expirationMinutes = 30)
        {
            TimeSpan difference = DateTime.Now - otpGenerationTime;
            return difference.TotalMinutes > expirationMinutes;
        }
    }
}
