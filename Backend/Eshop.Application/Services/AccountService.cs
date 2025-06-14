using Application.Helpers;
using AutoMapper;
using Eshop.Application.DTOs;
using Eshop.Application.DTOs.Profile;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Helpers;
using Eshop.Application.Interfaces.Repository;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;
using Eshop.Core.Enums;

namespace Eshop.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IMapper mapper;

        public AccountService(IAccountRepository accountRepository, IMapper mapper)
        {
            this.accountRepository = accountRepository;
            this.mapper = mapper;
        }


        public async Task<GeneralResponse<AuthResponseDTO>> SellerRegisterAsync(SellerRegisterDTO registerDto)
        {

            var TaxRegisterPdfURL = await DocumentSavingHelper.SaveOneDocumentAsync(registerDto.TaxRegisterPDFFile, "TaxRegisterDocs");
            registerDto.TaxRegisterPDF = TaxRegisterPdfURL;
            var seller = mapper.Map<Seller>(registerDto);
            seller.Role = UserRole.Seller;
            var registrationResult = await accountRepository.SellerRegisterAsync(seller, registerDto);

            return new GeneralResponse<AuthResponseDTO>
            {
                Data = registrationResult,
                Message = registrationResult.Message,
                Succeeded = registrationResult.Succeeded,
                Errors = registrationResult.Errors
            };
        }
        public async Task<GeneralResponse<AuthResponseDTO>> CustomerRegisterAsync(CustomerRegisterDTO RegisterModel)
        {
            var customer = mapper.Map<Customer>(RegisterModel);
            customer.Role = UserRole.Customer;
            var registrationResult = await accountRepository.CustomerRegisterAsync(customer, RegisterModel);

            return new GeneralResponse<AuthResponseDTO>
            {
                Data = registrationResult,
                Message = registrationResult.Message,
                Succeeded = registrationResult.Succeeded,
                Errors = registrationResult.Errors
            };
        }
        public async Task<GeneralResponse<AuthResponseDTO>> ConfirmEmailAsync(string email, string otp)
        {
            var result = await accountRepository.ConfirmEmailAsync(email, otp);

            return new GeneralResponse<AuthResponseDTO>
            {
                Data = result.IsEmailConfirmed ? result : null,
                Message = result.Message,
                Succeeded = result.Succeeded,
                Errors = result.Errors
            };
        }

        public async Task<GeneralResponse<bool>> ChangePasswordAsync(ChangePasswordDTO changePasswordModel, string userEmail)
        {
            var result = await accountRepository.ChangePasswordAsync(userEmail, changePasswordModel);

            return new GeneralResponse<bool>
            {
                Data = result.Succeeded,
                Message = result.Succeeded ? "Password changed successfully" : "Password change failed",
                Succeeded = result.Succeeded,
                Errors = result.Succeeded ? null : result.Errors.Select(e => e.Description).ToList()
            };
        }
        public async Task<GeneralResponse<bool>> ResetPasswordAsync(string userEmail, string token, string Password)
        {
            var result = await accountRepository.ResetPasswordAsync(userEmail, token, Password);

            return new GeneralResponse<bool>
            {
                Data = result.Succeeded,
                Message = result.Succeeded ? "Password changed successfully" : "Password change failed",
                Succeeded = result.Succeeded,
                Errors = result.Succeeded ? null : result.Errors.Select(e => e.Description).ToList()
            };
        }

        public async Task<GeneralResponse<string>> ForgetPassword(string Email)
        {
            var result = await accountRepository.ForgetPassword(Email);
            if (result)
            {
                return new GeneralResponse<string>()
                {
                    Data = "Reset Password Link sent successfully",
                    Succeeded = true
                };
            }
            return new GeneralResponse<string>()
            {
                Data = "Failed to send THe Reset Password Link",
                Succeeded = false
            };
        }

        public async Task<GeneralResponse<AuthResponseDTO>> Login(LoginUserDTO loginUser)
        {
            var LoginResult = await accountRepository.Login(loginUser);
            return new GeneralResponse<AuthResponseDTO>
            {
                Data = LoginResult,
                Message = LoginResult.Message,
                Succeeded = LoginResult.Succeeded,
                Errors = LoginResult.Errors

            };
        }


        public async Task<GeneralResponse<string>> SendNewOTPAsync(string email)
        {
            var result = await accountRepository.SendNewOTPAsync(email);
            if (result)
            {
                return new GeneralResponse<string>
                {
                    Data = "OTP sent successfully",
                    Succeeded = true,
                };
            }
            else
            {
                return new GeneralResponse<string>
                {
                    Message = "Failed to send OTP",
                    Succeeded = false
                };
            }
        }


        public async Task<GeneralResponse<GetSellerProfileDTO>> GetSellerProfileInfoAsync(string id)
        {
            var seller = await accountRepository.GetSellerByIdAsync(id);
            if (seller == null)
            {
                return new GeneralResponse<GetSellerProfileDTO>
                {
                    Data = null,
                    Message = "Seller not found",
                    Succeeded = false
                };
            }

            var sellerProfile = mapper.Map<GetSellerProfileDTO>(seller);
            return new GeneralResponse<GetSellerProfileDTO>
            {
                Data = sellerProfile,
                Message = "Profile retrieved successfully",
                Succeeded = true
            };
        }
        public async Task<GeneralResponse<GetCustomerProfileDTO>> GetCustomerProfileInfoAsync(string id)
        {
            //GetCustomerByIdAsync
            var customer = await accountRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return new GeneralResponse<GetCustomerProfileDTO>
                {
                    Data = null,
                    Message = "Customer not found",
                    Succeeded = false
                };
            }

            var customerProfile = mapper.Map<GetCustomerProfileDTO>(customer);
            return new GeneralResponse<GetCustomerProfileDTO>
            {
                Data = customerProfile,
                Message = "Profile retrieved successfully",
                Succeeded = true
            };
        }
        public async Task<GeneralResponse<bool>> UpdateSellerProfileAsync(string id, UpdateSellerProfileDTO updateDto)
        {

            var seller = await accountRepository.GetSellerByIdAsync(id);
            if (seller == null)
            {
                return new GeneralResponse<bool>
                {
                    Data = false,
                    Message = "Seller not found",
                    Succeeded = false
                };
            }

            if (updateDto.ProfileImage != null)
            {
                updateDto.imgURL = await ImageSavingHelper.SaveOneImageAsync(updateDto.ProfileImage, "SellersImages");
            }
            else
            {
                updateDto.imgURL = seller.ProfileImage;
            }


            mapper.Map(updateDto, seller);

            var updateResult = await accountRepository.UpdateSellerAsync(seller);
            if (updateResult)
            {
                return new GeneralResponse<bool>
                {
                    Data = true,
                    Message = "Profile updated successfully",
                    Succeeded = true
                };
            }

            return new GeneralResponse<bool>
            {
                Data = false,
                Message = "Profile update failed",
                Succeeded = false
            };
        }

        public async Task<GeneralResponse<string>> UpdateCustomerProfileAsync(string id, UpdateCustomerProfileDTO updateDto)
        {
            var customer = await accountRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return new GeneralResponse<string>
                {
                    Data = "NotFount",
                    Message = "customer not found",
                    Succeeded = false
                };
            }

            if (updateDto.ProfileImage != null)
            {
                updateDto.imgURL = await ImageSavingHelper.SaveOneImageAsync(updateDto.ProfileImage, "CustomerImages");
            }
            else
            {
                updateDto.imgURL = customer.ProfileImage;
            }


            mapper.Map(updateDto, customer);

            var updateResult = await accountRepository.UpdateCustomerAsync(customer);
            if (updateResult != "faild")
            {
                return new GeneralResponse<string>
                {
                    Data = updateResult,
                    Message = "Profile updated successfully",
                    Succeeded = true
                };
            }

            return new GeneralResponse<string>
            {
                Data = "failed",
                Message = "Profile update failed",
                Succeeded = false
            };
        }
    }
}
