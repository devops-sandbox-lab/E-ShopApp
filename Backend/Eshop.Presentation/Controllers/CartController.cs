using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Eshop.Application.DTOs.Cart;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Services;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService cartService;

        public CartController(ICartService cartService)
        {
            this.cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult> GetCart(int page = 1, int pageSize = 5)
        {
            var CustomerID = User.FindFirstValue("uid");
            try
            {

                var reslult = await cartService.GetCartByUserIdAsync(CustomerID, page, pageSize);

                if (reslult.Succeeded)
                {
                    return Ok(reslult);
                }
                else
                {
                    return BadRequest(reslult);
                }
            }
            catch (Exception ex)
            {


                return StatusCode(500, new GeneralResponse<bool>
                {
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }

        }

        //here should i check when i select an product to add if this product has sizes using the static helper function 
        //if it has should i show to the user all the size available to user and then the user should select the size and the quantity of this size 
        [HttpPost("addItem")]
        public async Task<ActionResult> AddToCart([FromBody] AddToCartDTO addToCartDto)
        {
            var CustomerID = User.FindFirstValue("uid");
            try
            {

                var reslult = await cartService.AddToCartAsync(addToCartDto, CustomerID);

                if (reslult.Succeeded)
                {
                    return Ok(reslult);
                }
                else
                {
                    return BadRequest(reslult);
                }
            }
            catch (Exception ex)
            {


                return StatusCode(500, new GeneralResponse<bool>
                {
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }

        }

        [HttpPut("removeItem")]
        public async Task<ActionResult> RemoveItem([FromBody] RemoveFromCartDto removeFromCartDto)
        {
            try
            {
                var CustomerID = User.FindFirstValue("uid");
                var reslult = await cartService.RemoveFromCartAsync(removeFromCartDto, CustomerID);

                if (reslult.Succeeded)
                {
                    return Ok(reslult);
                }
                else
                {
                    return BadRequest(reslult);
                }
            }
            catch (Exception ex)
            {


                return StatusCode(500, new GeneralResponse<bool>
                {
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }

        }
    }
}
