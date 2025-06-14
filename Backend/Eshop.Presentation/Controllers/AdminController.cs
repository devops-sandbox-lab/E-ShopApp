using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Eshop.Application.DTOs.Profile;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;
using Eshop.Core.Enums;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService adminService;
        private readonly UserManager<ApplicationUser> userManager;

        public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager)
        {
            this.adminService = adminService;
            this.userManager = userManager;
        }

        [HttpPut("acceptSeller")]
        public async Task<ActionResult> AcceptSellerAccount(string sellerId)
        {
            var currentUserLoginId = User.FindFirstValue("uid");

            var admin = await userManager.FindByIdAsync(currentUserLoginId);
            if (admin.Role != UserRole.Admin)
            {
                var errorResponse = new GeneralResponse<List<string>>
                {
                    Data = null,
                    Message = "Only Admin Can access here",
                    Succeeded = false,
                    Errors = new List<string> { "Only Admin Can access here" }
                };
                return BadRequest(errorResponse);
            }

            try
            {
                var result = await adminService.AcceptSellerAccount(sellerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResponse = new GeneralResponse<List<string>>
                {
                    Data = null,
                    Message = "error while accepting the seller",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
                return BadRequest(errorResponse);
            }

        }

        [HttpPut("DeclineSeller")]
        public async Task<ActionResult> DeclineSellerAccount(string sellerId)
        {


            try
            {
                var result = await adminService.DeclineSellerAccount(sellerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResponse = new GeneralResponse<List<string>>
                {
                    Data = null,
                    Message = "error while declining the seller",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
                return BadRequest(errorResponse);
            }

        }

        [HttpGet("GetAllSellers")]
        public async Task<ActionResult<GeneralResponse<List<GetSellerProfileDTO>>>> GetAllSellers(
            int page = 1,
            int pageSize = 6,
            SellerAccountStatus? status = null,
            bool? isBlocked = null)
        {

            try
            {
                var response = await adminService.GetAllSellers(page, pageSize, status, isBlocked);

                if (response.Succeeded)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GeneralResponse<List<GetSellerProfileDTO>>
                {
                    Data = null,
                    Message = "An error occurred while retrieving sellers.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
