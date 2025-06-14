using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Eshop.Application.DTOs;
using Eshop.Application.DTOs.Profile;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly IAccountService accountService;
        private readonly IGoogleAuthService googleAuthService;

        public AccountController(UserManager<ApplicationUser> _userManager, IConfiguration _config, IAccountService accountService, IGoogleAuthService googleAuthService)

        {
            userManager = _userManager;
            config = _config;
            this.accountService = accountService;
            this.googleAuthService = googleAuthService;

        }


        [HttpPost("SellerRegister")]
        public async Task<ActionResult> SellerRegister([FromForm] SellerRegisterDTO SellerRegisterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await accountService.SellerRegisterAsync(SellerRegisterModel);

            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message, errors = response.Errors });
            }
        }


        [HttpPost("customerRegister")]
        public async Task<ActionResult> CustomerRegister([FromForm] CustomerRegisterDTO customerRegisterModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await accountService.CustomerRegisterAsync(customerRegisterModel);

            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message, errors = response.Errors });
            }
        }

        [HttpPost("confirmEmail")]
        public async Task<ActionResult> ConfirmEmail(string otp)
        {
            string Email = User.FindFirstValue(ClaimTypes.Email);

            var result = await accountService.ConfirmEmailAsync(Email, otp);

            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("forgetPassword")]
        public async Task<ActionResult> ForgetPassword(string Email)
        {
            var result = await accountService.ForgetPassword(Email);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);

        }
        [HttpGet("resendOTP")]
        public async Task<ActionResult> GetNewOTP()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return BadRequest(new { message = "User Not Found , Please Login" });

            }
            var result = await accountService.SendNewOTPAsync(email);

            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginUserDTO loginUserModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await accountService.Login(loginUserModel);

            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message, errors = response.Errors });
            }
        }


        [HttpPost("changePassword")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordDTO changePasswordDto)
        {

            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return BadRequest(new { message = "User Not Found , Please Login" });

            }
            var response = await accountService.ChangePasswordAsync(changePasswordDto, email);

            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message, errors = response.Errors });
            }
        }

        [HttpPost("resetPassword")]
        [Authorize]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var response = await accountService.ResetPasswordAsync(email, resetPasswordDTO.token, resetPasswordDTO.newPassword);

            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message, errors = response.Errors });
            }
        }


        [HttpPost("googleLogin")]
        public async Task<ActionResult> GoogleSignIn([FromBody] GoogleTokenDTO googleTokenDto)
        {
            var result = await googleAuthService.GoogleSignIn(googleTokenDto.googleToken);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("sellerInfo")]
        public async Task<ActionResult> GetSellerProfileInfo([FromQuery] string sellerId = "null")

        {
            if (sellerId == "null")
            {
                sellerId = User.FindFirstValue("uid");

            }

            var sellerProfile = await accountService.GetSellerProfileInfoAsync(sellerId);
            if (sellerProfile == null)
            {
                return NotFound();
            }
            return Ok(sellerProfile);
        }

        [HttpGet("customerInfo")]
        public async Task<ActionResult> GetCustomerProfileInfo([FromQuery] string customerId = "null")

        {
            if (customerId == "null")
            {
                customerId = User.FindFirstValue("uid");

            }


            var customerProfile = await accountService.GetCustomerProfileInfoAsync(customerId);
            if (customerProfile == null)
            {
                return NotFound();
            }
            return Ok(customerProfile);
        }

        [HttpPut("sellerInfo")]
        public async Task<ActionResult> UpdateSellerProfile([FromForm] UpdateSellerProfileDTO updateDto)
        {

            string id = User.FindFirstValue("uid");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accountService.UpdateSellerProfileAsync(id, updateDto);
            if (!result.Succeeded)
            {
                return NotFound();
            }

            return NoContent();
        }


        [HttpPut("customerInfo")]
        public async Task<ActionResult> UpdateCustomerProfile([FromForm] UpdateCustomerProfileDTO updateDto)
        {

            string id = User.FindFirstValue("uid");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await accountService.UpdateCustomerProfileAsync(id, updateDto);
            if (!result.Succeeded)
            {
                return NotFound();
            }

            return Ok(result);
        }

    }
}
