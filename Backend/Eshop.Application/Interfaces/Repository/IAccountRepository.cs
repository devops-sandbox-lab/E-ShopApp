using Microsoft.AspNetCore.Identity;
using Eshop.Application.DTOs;
using Eshop.Core.Entities;

namespace Eshop.Application.Interfaces.Repository
{
    public interface IAccountRepository
    {

        public Task<AuthResponseDTO> SellerRegisterAsync(Seller owner, SellerRegisterDTO registerDto);
        public Task<AuthResponseDTO> CustomerRegisterAsync(Customer customer, CustomerRegisterDTO RegisterModel);
        public Task<AuthResponseDTO> Login(LoginUserDTO loginUser);
        public Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDTO changePasswordModel);
        public Task<IdentityResult> ResetPasswordAsync(string userEmail, string token, string Password);
        public Task<AuthResponseDTO> ConfirmEmailAsync(string email, string otp);
        public Task<bool> SendNewOTPAsync(string email);
        public Task<bool> ForgetPassword(string Email);
        public Task<AuthResponseDTO> GoogleSignIn(string model);
        public Task<Seller> GetSellerByIdAsync(string id);
        public Task<Customer> GetCustomerByIdAsync(string id);

        public Task<bool> UpdateSellerAsync(Seller seller);

        public Task<string> UpdateCustomerAsync(Customer customer);
        public Task save();

    }
}
