using Eshop.Application.DTOs;
using Eshop.Application.DTOs.Profile;
using Eshop.Application.GeneralResponse;

namespace Eshop.Application.Interfaces.Services
{
    public interface IAccountService
    {
        public Task<GeneralResponse<AuthResponseDTO>> SellerRegisterAsync(SellerRegisterDTO RegisterModel);
        public Task<GeneralResponse<AuthResponseDTO>> CustomerRegisterAsync(CustomerRegisterDTO RegisterModel);
        public Task<GeneralResponse<AuthResponseDTO>> Login(LoginUserDTO loginUser);
        public Task<GeneralResponse<bool>> ChangePasswordAsync(ChangePasswordDTO changePasswordModel, string userEmail);
        public Task<GeneralResponse<bool>> ResetPasswordAsync(string userEmail, string token, string Password);
        public Task<GeneralResponse<string>> SendNewOTPAsync(string email);
        public Task<GeneralResponse<string>> ForgetPassword(string Email);
        public Task<GeneralResponse<AuthResponseDTO>> ConfirmEmailAsync(string email, string otp);

        Task<GeneralResponse<GetSellerProfileDTO>> GetSellerProfileInfoAsync(string id);
        Task<GeneralResponse<GetCustomerProfileDTO>> GetCustomerProfileInfoAsync(string id);
        Task<GeneralResponse<bool>> UpdateSellerProfileAsync(string id, UpdateSellerProfileDTO updateDto);
        Task<GeneralResponse<string>> UpdateCustomerProfileAsync(string id, UpdateCustomerProfileDTO updateDto);



    }
}
