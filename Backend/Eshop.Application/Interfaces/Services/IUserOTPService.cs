using Eshop.Application.DTOs;

namespace Eshop.Application.Interfaces.Services
{
    public interface IUserOTPService
    {
        public Task SaveAndSendOTPAsync(string email, string firstName, string lastName);
        public Task<bool> VerifyOTPAsync(string email, string otp);
        public Task<AuthResponseDTO> SendNewOTPAsync(string email, string firstName, string lastName);
        public Task<bool> SendForgetPasswordLinkAsync(string email, string token, string firstName, string lastName);

    }
}
