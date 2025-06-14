using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Eshop.Application.Interfaces.Repository;
using Eshop.Core.Entities;

namespace Eshop.Infrastructure.Repositories
{
    public class UserOTPRepository : IUserOTPRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserOTPRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }

        public async Task SaveOTPAsync(UserOTP userOTP)
        {
            UserOTP otp = _context.userOTPs.FirstOrDefault(otp => otp.Email == userOTP.Email);
            if (otp == null)
            {
                _context.userOTPs.Add(userOTP);

            }
            else
            {
                otp.OTP = userOTP.OTP;
                _context.userOTPs.Update(otp);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<UserOTP> GetOTPAsync(string email, string otp)
        {
            return await _context.userOTPs
                .FirstOrDefaultAsync(u => u.Email == email && u.OTP == otp);


        }

        public async Task DeleteOTPAsync(UserOTP userOTP)
        {
            _context.userOTPs.Remove(userOTP);
            await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUser> FindUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }


    }

}
