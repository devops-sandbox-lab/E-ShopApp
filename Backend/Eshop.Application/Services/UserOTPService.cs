using Eshop.Application.DTOs;
using Eshop.Application.Helpers;
using Eshop.Application.Interfaces.Repository;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;

namespace Eshop.Application.Services
{
    public class UserOTPService : IUserOTPService
    {
        private readonly IUserOTPRepository _userOTPRepository;
        private readonly IEmailService emailService;

        public UserOTPService(IUserOTPRepository userOTPRepository, IEmailService emailService)
        {
            _userOTPRepository = userOTPRepository;
            this.emailService = emailService;
        }

        public async Task SaveAndSendOTPAsync(string email, string firstName, string lastName)
        {
            var otp = GenerateRandomCode.GetCode();

            var userOTP = new UserOTP
            {
                OTP = otp,
                Email = email,
                OTPGeneratedTime = DateTime.Now
            };

            await _userOTPRepository.SaveOTPAsync(userOTP);

            EmailDTO emailDTO = new EmailDTO
            {
                To = email,
                Subject = "Eshop Account Verification OTP",
                Body = FormatEmail.ConfirmEmail(otp, $"{firstName} {lastName}", DateTime.Now.ToString("dd MMM, yyyy"))
            };
            await emailService.sendEmailAsync(emailDTO);
        }

        public async Task<bool> VerifyOTPAsync(string email, string otp)
        {
            var userOTP = await _userOTPRepository.GetOTPAsync(email, otp);
            if (userOTP == null)
            {
                return false;
            }
            var IsExpire = CalcOTPExpirationTime.IsOTPExpired(userOTP.OTPGeneratedTime);
            if (userOTP == null || IsExpire)
            {
                if (userOTP != null && IsExpire)
                {
                    await _userOTPRepository.DeleteOTPAsync(userOTP);
                }
                return false;
            }
            await _userOTPRepository.DeleteOTPAsync(userOTP);

            return true;
        }


        public async Task<AuthResponseDTO> SendNewOTPAsync(string email, string firstName, string lastName)
        {
            var otp = GenerateRandomCode.GetCode();

            var userOTP = new UserOTP
            {
                OTP = otp,
                Email = email,
                OTPGeneratedTime = DateTime.Now
            };

            await _userOTPRepository.SaveOTPAsync(userOTP);

            EmailDTO emailDTO = new EmailDTO
            {
                To = email,
                Subject = "Eshop Account Verification OTP",
                Body = FormatEmail.ConfirmEmail(otp, $"{firstName} {lastName}", DateTime.Now.ToString("dd MMM, yyyy"))
            };
            await emailService.sendEmailAsync(emailDTO);

            return new AuthResponseDTO { Message = "New OTP sent to your email successfully" };
        }
        public async Task<bool> SendForgetPasswordLinkAsync(string email, string token, string firstName, string lastName)
        {

            EmailDTO emailDTO = new EmailDTO
            {
                To = email,
                Subject = "Reset Password For Eshop Account ]",
                Body = FormatEmail.ForgetPassword(token, $"{firstName} {lastName}", DateTime.Now.ToString("dd MMM, yyyy"))
            };
            try
            {
                await emailService.sendEmailAsync(emailDTO);

            }
            catch
            {
                return false;

            }

            return true;
        }
    }
}

