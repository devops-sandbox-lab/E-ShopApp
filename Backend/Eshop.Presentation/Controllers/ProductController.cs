using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Eshop.Application.DTOs.Product;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Helpers;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly UserManager<ApplicationUser> userManager;

        public ProductController(IProductService productService, UserManager<ApplicationUser> userManager)
        {
            this.productService = productService;
            this.userManager = userManager;
        }
        [HttpGet("GetSizeName/{productSizeId}")]
        public async Task<IActionResult> GetSizeNameByProductSizeId(int productSizeId)
        {
            var response = await productService.GetSizeNameByProductSizeId(productSizeId);

            if (response.Succeeded)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpGet("getSizes")]
        public async Task<ActionResult> GetSizes()
        {
            var response = await productService.GetAllSizesAsync();
            if (response.Succeeded)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        [HttpGet("get-product-sizes")]
        public async Task<ActionResult> GerProductSizes(int productId)
        {
            try
            {
                var result = await this.productService.GetProductSizeDetailsAsync(productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpDelete("deleteSize/{sizeId}/{productId}")]
        public async Task<IActionResult> DeleteSize(int sizeId, int productId)
        {
            if (sizeId <= 0 || productId <= 0)
            {
                return BadRequest("Invalid size ID or product ID.");
            }

            var result = await productService.DeleteSizeAsync(sizeId, productId);

            if (result.Succeeded)
            {
                return NoContent(); // Success, no content to return
            }
            else
            {
                return NotFound("Size not found.");
            }
        }


        [HttpPost("addProduct")]
        public async Task<ActionResult> AddProduct([FromForm] AddProductDTO productDTO)
        {
            string SellerId = User.FindFirstValue("uid");
            productDTO.sellerId = SellerId;


            if (ProductSizeHelper.RequiresSizes(productDTO.CategoryId) && (productDTO.productSizesJson is null))
            {
                return BadRequest("Product sizes are required for clothing categories.");
            }

            if (!ProductSizeHelper.RequiresSizes(productDTO.CategoryId) && !productDTO.StockQuantity.HasValue)
            {
                return BadRequest("Stock quantity is required for non-clothing products.");
            }
            var result = await productService.AddProduct(productDTO);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpPost("importProducts")]
        public async Task<ActionResult> ImportProducts([FromForm] List<AddProductDTO> productDTOs)
        {
            if (productDTOs == null || !productDTOs.Any())
            {
                var errorResponse = new GeneralResponse<List<ProductDTO>>
                {
                    Data = null,
                    Message = "No products to import. The list is either null or empty.",
                    Succeeded = false,
                    Errors = new List<string> { "The provided product list is empty or null." }
                };
                return BadRequest(errorResponse);
            }

            string SellerId = User.FindFirstValue("uid");

            foreach (var productDTO in productDTOs)
            {
                productDTO.sellerId = SellerId;
            }

            var result = await productService.AddBulkProducts(productDTOs);

            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }



        [HttpPut("editProduct/{id}")]
        public async Task<ActionResult> EditProduct(int id, [FromForm] EditProductDTO productDTO)
        {
            string sellerId = User.FindFirstValue("uid");
            productDTO.sellerId = sellerId;

            // You would typically fetch the existing product by its ID and update its properties here
            var existingProduct = await productService.GetProductById(id);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            if (ProductSizeHelper.RequiresSizes(productDTO.CategoryId) && (productDTO.productSizesJson == null))
            {
                return BadRequest("Product sizes are required for clothing categories.");
            }

            if (!ProductSizeHelper.RequiresSizes(productDTO.CategoryId) && !productDTO.StockQuantity.HasValue)
            {
                return BadRequest("Stock quantity is required for non-clothing products.");
            }

            var result = await productService.EditProduct(productDTO, id);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }




        [HttpPost("addFeatures")]
        public async Task<ActionResult> AddFeatures([FromBody] List<AddProductFeaturesDTO> addProductFeatures)
        {
            var result = await productService.AddProductFeatures(addProductFeatures);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPut("editFeatures")]
        public async Task<ActionResult> EditFeatures([FromBody] List<EditFeatureDTO> productFeatureDTOs)
        {
            var result = await productService.EditProductFeatures(productFeatureDTOs);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpDelete("deleteFeature/{id}")]
        public async Task<ActionResult> DeleteFeature(int id)
        {
            if (id == null)
            {
                return BadRequest(new { Message = "Feature IDs are required." });
            }

            // Proceed with deletion logic
            var result = await productService.DeleteProductFeature(id);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }



        [HttpDelete("deleteProduct/{productId}")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {
            // Get the list of image IDs associated with the product
            var imageIdsResponse = await productService.GetProductImageIds(productId);

            if (!imageIdsResponse.Succeeded)
            {
                return BadRequest(imageIdsResponse.Message);
            }

            if (imageIdsResponse.Data == null || imageIdsResponse.Data.Count == 0)
            {
                return NotFound("No images found for the product.");
            }

            // Delete the images first
            var deleteImagesResult = await productService.DeleteProductImages(imageIdsResponse.Data);
            if (!deleteImagesResult.Succeeded)
            {
                return BadRequest(deleteImagesResult.Message);
            }

            // Delete the product
            var deleteProductResult = await productService.DeleteProduct(productId);
            if (deleteProductResult.Succeeded)
            {
                return Ok(deleteProductResult.Message);
            }
            else
            {
                return BadRequest(deleteProductResult.Message);
            }
        }


        [HttpDelete("deleteImages")]
        public async Task<ActionResult> DeleteImages([FromBody] List<int> imageIds)
        {
            var result = await productService.DeleteProductImages(imageIds);
            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("getSellerProducts")]
        public async Task<ActionResult> GetSellerProducts(int page = 1, int pageSize = 10)
        {
            try
            {
                string sellerId = User.FindFirstValue("uid");

                if (string.IsNullOrEmpty(sellerId))
                {
                    return BadRequest(new GeneralResponse<List<GetProductsDTO>>
                    {
                        Data = null,
                        Message = "Seller ID is not found in the user claims.",
                        Succeeded = false,
                        Errors = new List<string> { "Seller ID is missing." }
                    });
                }

                var result = await this.productService.GetAllSellerProducts(sellerId, page, pageSize);
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


                return StatusCode(500, new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = null,
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }
        [HttpGet("getCategoryProducts")]
        public async Task<ActionResult> GetCategoryProducts(
    [FromQuery] int categoryID,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string searchTerm = "",
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null,
    [FromQuery] string sortOption = "",
    [FromQuery] int subcategoryId = 0
            )
        {
            try
            {
                var result = await this.productService.GetProductsByMainCategory(
                    categoryID, page, pageSize, searchTerm, minPrice, maxPrice, sortOption, subcategoryId);
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
                return StatusCode(500, new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = null,
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }
        [HttpGet("getSubCategoryProducts")]
        public async Task<ActionResult> GetSubCategoryProducts(
   [FromQuery] int subCategoryID,
   [FromQuery] int page = 1,
   [FromQuery] int pageSize = 10,
   [FromQuery] string searchTerm = "",
   [FromQuery] decimal? minPrice = null,
   [FromQuery] decimal? maxPrice = null,
   [FromQuery] string sortOption = "")
        {
            try
            {
                var result = await this.productService.GetProductsBySubCategory(
                    subCategoryID, page, pageSize, searchTerm, minPrice, maxPrice, sortOption);
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
                return StatusCode(500, new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = null,
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }
        [HttpGet("getProductById")]
        public async Task<ActionResult> GetProductById(int productId)
        {
            string CustomerId = User.FindFirstValue("uid");

            try
            {
                var result = await productService.GetProductById(productId, CustomerId);
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
                return StatusCode(500, new GeneralResponse<GetProductsDTO>
                {
                    Data = null,
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }






        [HttpGet("getSearchProducts")]
        public async Task<ActionResult> GetSearchProducts(
 [FromQuery] int page = 1,
 [FromQuery] int pageSize = 10,
 [FromQuery] string searchTerm = "",
 [FromQuery] decimal? minPrice = null,
 [FromQuery] decimal? maxPrice = null,
 [FromQuery] string sortOption = ""
         )
        {
            try
            {
                var result = await this.productService.GetSearchProduct(
                     page, pageSize, searchTerm, minPrice, maxPrice, sortOption);
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
                return StatusCode(500, new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = null,
                    Message = "An internal server error occurred.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                });
            }
        }


        [HttpGet("getRecommendedProducts")]
        public async Task<IActionResult> GetRecommendedProducts()
        {
            var recommendedProducts = await productService.GetRecommendedProductsAsync();
            return Ok(recommendedProducts);
        }

        [HttpGet("getDealProducts")]
        public async Task<IActionResult> GetDealProducts()
        {
            var recommendedProducts = await productService.GetDealProductsAsync();
            return Ok(recommendedProducts);
        }
        [HttpGet("getBestSellerProducts")]
        public async Task<IActionResult> GetBestSellerProducts()
        {
            var recommendedProducts = await productService.GetBestSellerProductsAsync();
            return Ok(recommendedProducts);
        }
        [HttpGet("getRandomProducts")]
        public async Task<IActionResult> GetRandomProductsAsync()
        {
            var response = await productService.GetRandomProductsAsync();
            if (response.Succeeded)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("get-more-from-seller-Products")]
        public async Task<IActionResult> GetMoreFromSellerAsync(string sellerId)
        {
            var response = await productService.GetMoreFromSellerProductsAsync(sellerId);
            if (response.Succeeded)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }


        [HttpGet("get-more-from-same-category-Products")]
        public async Task<IActionResult> GetMoreFromCategoryAsync(int categoryId)
        {
            var response = await productService.GetMoreFromCategoryProductsAsync(categoryId);
            if (response.Succeeded)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

    }
}
