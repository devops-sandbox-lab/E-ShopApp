using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService FavoriteService;
        private readonly IProductService productService;

        public FavoriteController(IFavoriteService _favoriteService, IProductService productService)
        {
            FavoriteService = _favoriteService;
            this.productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pagesize = 5)
        {
            string CustomerID = User.FindFirstValue("uid");
            try
            {
                var allFavIds = await FavoriteService.GetAllFavIds(CustomerID);

                var response = await productService.GetProductsByIds(allFavIds, page, pagesize);
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
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Add/{productID}")]
        public async Task<IActionResult> AddServiceToFav(int productID)
        {
            // Assuming you get the customer ID from the JWT token or similar mechanism
            string customerID = User.FindFirstValue("uid");

            try
            {
                // Call your service method to add the product to the favorites
                var result = await FavoriteService.AddProductToFav(productID, customerID);

                if (result.Succeeded)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GeneralResponse<bool>
                {
                    Data = false,
                    Message = "Failed To Add Product To Fav",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }



        [HttpDelete("Remove/{productID}")]
        public IActionResult RemoveServiceFromFav(int productID)
        {
            string CustomerID = User.FindFirstValue("uid");

            try
            {
                FavoriteService.RemoveProductFromFav(productID, CustomerID);
                return Ok(new GeneralResponse<Product>
                {
                    Data = null,
                    Message = "Product Removed Successfully",
                    Succeeded = true,
                    Errors = null
                });
            }
            catch (Exception ex)
            {

                return NotFound(new GeneralResponse<Product>
                {
                    Data = null,
                    Message = "Failed To Remove Product",
                    Succeeded = true,
                    Errors = null
                });
            }
        }
    }
}
