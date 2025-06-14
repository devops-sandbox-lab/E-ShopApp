using Application.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Eshop.Application.DTOs.Product;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Services;
using Eshop.Application.Interfaces.UnitOfWork;
using Eshop.Core.Entities;

namespace Eshop.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        public async Task<GeneralResponse<string>> GetSizeNameByProductSizeId(int productSizeId)
        {
            try
            {
                // Retrieve the ProductSize entity including the related Size entity
                var productSize = await unitOfWork.GetRepository<ProductSize>()
                                                  .FindAsync(s => s.Id == productSizeId, ["Size"]);

                if (productSize != null && productSize.Size != null)
                {
                    // Return the size name
                    return new GeneralResponse<string>
                    {
                        Data = productSize.Size.Name,
                        Succeeded = true
                    };
                }
                else
                {
                    return new GeneralResponse<string>
                    {
                        Message = "Size not found for the given ProductSizeId.",
                        Succeeded = false
                    };
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                return new GeneralResponse<string>
                {
                    Message = $"An error occurred: {ex.Message}",
                    Succeeded = false
                };
            }
        }

        public async Task<GeneralResponse<List<GetSizeDTO>>> GetAllSizesAsync()
        {



            try
            {
                var result = await unitOfWork.GetRepository<Size>().FindAllAsync();
                var SizeDTOs = mapper.Map<List<GetSizeDTO>>(result);
                return new GeneralResponse<List<GetSizeDTO>>
                {
                    Data = SizeDTOs,
                    Succeeded = true,
                    Message = "Sizes retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<GetSizeDTO>>
                {
                    Data = null,
                    Succeeded = false,
                    Message = $"An error occurred while retrieving Sizes: {ex.Message}",
                    Errors = new List<string> { ex.Message }

                };
            }
        }

        public async Task<GeneralResponse<List<ProductSizeDetailDTO>>> GetProductSizeDetailsAsync(int productId)
        {
            try
            {
                var productSizes = await unitOfWork.GetRepository<ProductSize>()
                    .FindAllAsync(null, p => p.ProductId == productId);

                var productSizeDTOs = new List<ProductSizeDetailDTO>();

                foreach (var productSize in productSizes)
                {
                    var size = await unitOfWork.GetRepository<Size>().FindAsync(s => s.SizeId == productSize.SizeId);
                    var productSizeDTO = new ProductSizeDetailDTO
                    {
                        SizeId = productSize.SizeId,
                        StockQuantity = productSize.StockQuantity,
                        Price = productSize.Price,
                        Name = size?.Name
                    };

                    productSizeDTOs.Add(productSizeDTO);
                }

                return new GeneralResponse<List<ProductSizeDetailDTO>>
                {
                    Data = productSizeDTOs,
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<ProductSizeDetailDTO>>
                {
                    Succeeded = false,
                    Message = "An error occurred while retrieving product sizes.",
                    Errors = new List<string> { ex.Message, ex.StackTrace }
                };
            }
        }




        public async Task<GeneralResponse<bool>> DeleteSizeAsync(int sizeId, int productId)
        {
            var response = new GeneralResponse<bool>();

            var size = await unitOfWork.GetRepository<ProductSize>()
                .FindAsync(s => s.SizeId == sizeId && s.ProductId == productId);
            if (size == null)
            {
                response.Succeeded = false;
                response.Message = "Size not found.";
                response.Errors = new List<string> { "Size not found." };
                return response;
            }

            await unitOfWork.GetRepository<ProductSize>().DeleteAsync(size.Id);
            await unitOfWork.SaveChangesAsync();

            response.Succeeded = true;
            response.Data = true;
            response.Message = "Size deleted successfully.";
            return response;
        }
        public async Task<GeneralResponse<ProductDTO>> AddProduct(AddProductDTO addProductDTO)
        {
            using (var transaction = await unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    Product product = mapper.Map<Product>(addProductDTO);

                    var imagePaths = await ImageSavingHelper.SaveImagesAsync(addProductDTO.productImages, "ProductImages");
                    product.productImages = imagePaths.Select(path => new ProductImages { ImageUrl = path }).ToList();
                    addProductDTO.productImagesUrl = imagePaths;

                    if (addProductDTO.productSizesJson is not null)
                    {
                        addProductDTO.ProductSizes = System.Text.Json.JsonSerializer.Deserialize<List<ProductSizeDTO>>(addProductDTO.productSizesJson);
                        product.ProductSizes = addProductDTO.ProductSizes.Select(ps => new ProductSize
                        {
                            SizeId = ps.SizeId,
                            StockQuantity = ps.StockQuantity,
                            Price = ps.Price,
                        }).ToList();
                    }
                    else
                    {
                        product.StockQuantity = addProductDTO.StockQuantity ?? 0;
                    }
                    product.Discount = addProductDTO.Discount ?? 0;

                    unitOfWork.productRepository.Add(product);

                    await unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var res = mapper.Map<ProductDTO>(product);

                    return new GeneralResponse<ProductDTO>
                    {
                        Data = res,
                        Message = "Product added successfully",
                        Succeeded = true,
                        Errors = null,
                        PaginationInfo = null
                    };
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();


                    var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                    return new GeneralResponse<ProductDTO>
                    {
                        Data = null,
                        Message = "Error while adding product",
                        Succeeded = false,
                        Errors = new List<string> { innerExceptionMessage }
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return new GeneralResponse<ProductDTO>
                    {
                        Data = null,
                        Message = "Unexpected error occurred",
                        Succeeded = false,
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
        }



        public async Task<GeneralResponse<bool>> AddBulkProducts(List<AddProductDTO> addProductDTOs)
        {
            var responses = new List<ProductDTO>();
            var errors = new List<string>();

            try
            {
                foreach (var addProductDTO in addProductDTOs)
                {
                    try
                    {

                        Product product = mapper.Map<Product>(addProductDTO);


                        var imagePaths = await ImageSavingHelper.SaveImagesAsync(addProductDTO.productImages, "ProductImages");
                        product.productImages = imagePaths.Select(path => new ProductImages { ImageUrl = path }).ToList();
                        addProductDTO.productImagesUrl = imagePaths;

                        if (addProductDTO.productSizesJson != null)
                        {
                            addProductDTO.ProductSizes = System.Text.Json.JsonSerializer.Deserialize<List<ProductSizeDTO>>(addProductDTO.productSizesJson);

                            product.ProductSizes = addProductDTO.ProductSizes.Select(ps => new ProductSize
                            {
                                SizeId = ps.SizeId,
                                StockQuantity = ps.StockQuantity,
                                Price = ps.Price,
                            }).ToList();
                        }
                        else
                        {
                            product.StockQuantity = addProductDTO.StockQuantity ?? 0;
                        }

                        unitOfWork.productRepository.Add(product);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error processing product {addProductDTO.Name}: {ex.Message}");
                    }
                }

                await unitOfWork.SaveChangesAsync();


                var response = new GeneralResponse<bool>
                {
                    Data = true,
                    Message = "Products added successfully",
                    Succeeded = true,
                    Errors = null,
                    PaginationInfo = null
                };

                return response;
            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                var errorResponse = new GeneralResponse<bool>
                {
                    Data = false,
                    Message = "Error while adding products",
                    Succeeded = false,
                    Errors = new List<string> { innerExceptionMessage }
                };
                return errorResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new GeneralResponse<bool>
                {
                    Data = false,
                    Message = "An unexpected error occurred",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
                return errorResponse;
            }
        }



        public async Task<GeneralResponse<List<AddProductFeaturesDTO>>> AddProductFeatures(List<AddProductFeaturesDTO> addProductFeatures)
        {
            try
            {
                var productId = addProductFeatures.First().ProductId;
                var existingFeatures = await unitOfWork.featureRepository.GetAllAsync(f => f.ProductId == productId);

                var newFeatures = addProductFeatures
                    .Where(f => !existingFeatures.Any(ef => ef.FeatureName == f.FeatureName && ef.FeatureValue == f.FeatureValue))
                    .ToList();

                if (!newFeatures.Any())
                {
                    return new GeneralResponse<List<AddProductFeaturesDTO>>
                    {
                        Data = null,
                        Message = "No new features to add",
                        Succeeded = false,
                        Errors = new List<string> { "All features already exist" }
                    };
                }

                var features = mapper.Map<List<Feature>>(newFeatures);
                await unitOfWork.featureRepository.AddRangeAsync(features);
                await unitOfWork.SaveChangesAsync();

                var responseFeatures = mapper.Map<List<AddProductFeaturesDTO>>(features);
                var response = new GeneralResponse<List<AddProductFeaturesDTO>>
                {
                    Data = responseFeatures,
                    Message = "Product features added successfully",
                    Succeeded = true,
                    Errors = null
                };

                return response;
            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                var errorResponse = new GeneralResponse<List<AddProductFeaturesDTO>>
                {
                    Data = null,
                    Message = "Error while adding product features",
                    Succeeded = false,
                    Errors = new List<string> { innerExceptionMessage }
                };
                return errorResponse;
            }
        }



        public async Task<GeneralResponse<List<EditFeatureDTO>>> EditProductFeatures(List<EditFeatureDTO> editFeatureDTOs)
        {
            try
            {
                var featureEntities = await unitOfWork.featureRepository.FindAllAsync(null,
                    f => editFeatureDTOs.Select(dto => dto.FeatureId).Contains(f.FeatureId));

                var featuresToUpdate = featureEntities.ToList();
                foreach (var feature in featuresToUpdate)
                {
                    var dto = editFeatureDTOs.FirstOrDefault(f => f.FeatureId == feature.FeatureId);
                    if (dto != null)
                    {
                        feature.FeatureName = dto.FeatureName;
                        feature.FeatureValue = dto.FeatureValue;
                    }
                }

                unitOfWork.featureRepository.UpdateRange(featuresToUpdate);
                await unitOfWork.SaveChangesAsync();
                var responseFeatures = mapper.Map<List<EditFeatureDTO>>(featuresToUpdate);
                var response = new GeneralResponse<List<EditFeatureDTO>>
                {
                    Data = responseFeatures,
                    Message = "Product features updated successfully",
                    Succeeded = true,
                    Errors = null
                };

                return response;

            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                var errorResponse = new GeneralResponse<List<EditFeatureDTO>>
                {
                    Data = null,
                    Message = "Error while updating product features",
                    Succeeded = false,
                    Errors = new List<string> { innerExceptionMessage }
                };
                return errorResponse;
            }
        }

        public async Task<GeneralResponse<bool>> DeleteProductFeature(int featureId)
        {
            try
            {
                await unitOfWork.featureRepository.DeleteAsync(featureId);
                await unitOfWork.SaveChangesAsync();
                var response = new GeneralResponse<bool>
                {
                    Data = true,
                    Message = "Product features deleted successfully",
                    Succeeded = true,
                    Errors = null
                };

                return response;

            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                var errorResponse = new GeneralResponse<bool>
                {
                    Data = false,
                    Message = "Error while deleting product features",
                    Succeeded = false,
                    Errors = new List<string> { innerExceptionMessage }
                };
                return errorResponse;
            }
        }

        public async Task<GeneralResponse<bool>> DeleteProductImages(List<int> imageIds)
        {
            try
            {
                await unitOfWork.productRepository.DeleteImagesAsync(imageIds);
                await unitOfWork.SaveChangesAsync();

                var response = new GeneralResponse<bool>
                {
                    Data = true,
                    Message = "Product images deleted successfully",
                    Succeeded = true,
                    Errors = null
                };

                return response;
            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                var errorResponse = new GeneralResponse<bool>
                {
                    Data = false,
                    Message = "Error while deleting product images",
                    Succeeded = false,
                    Errors = new List<string> { innerExceptionMessage }
                };
                return errorResponse;
            }
        }

        public async Task<GeneralResponse<Product>> GetProductById(int productId, string UserId = null)
        {
            var orders = await unitOfWork.orderRepository.FindAllAsync(
                  new[] { "OrderItems" },
                  order => order.BuyerId == UserId
              );
            try
            {


                var product = await unitOfWork.productRepository.FindAsync(p => p.ProductId == productId,
                new string[] { "productImages", "Features", "ProductSizes", "Reviews", "ProductSizes", "Seller" });


                bool hasPurchasedProduct = orders
                   .SelectMany(order => order.OrderItems)
                   .Any(item => item.ProductId == productId);

                var IsAddedReviewBefore = this.unitOfWork.GetRepository<Reviews>().Find(r => r.CustomerId == UserId && r.ProductId == productId);

                if (hasPurchasedProduct)
                {
                    product.isPreviouslyBought = true;
                }
                if (IsAddedReviewBefore is not null)
                {
                    product.IsAddedReviewBefore = true;
                }


                if (product == null)
                {
                    return new GeneralResponse<Product>
                    {
                        Data = null,
                        Message = "No products found.",
                        Succeeded = false,
                    };
                }


                product.IsFavorites = await unitOfWork.productRepository.IsFav(productId, UserId);
                return new GeneralResponse<Product>
                {
                    Data = product,
                    Message = "Product retrieved successfully.",
                    Succeeded = true,
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<Product>
                {
                    Data = null,
                    Message = "An error occurred while retrieving the product.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<GeneralResponse<List<GetProductsDTO>>> GetAllSellerProducts(string sellerId, int page = 1, int pageSize = 10)
        {
            try
            {

                var result = await unitOfWork.productRepository.FindAllByOrder(
                   new string[] { "productImages", "Features", "ProductSizes" },
                   p => p.sellerId == sellerId,
                   orderBy: p => p.OrderByDescending(prod => prod.CreatedAt)
                    );

                if (result == null || !result.Any())
                {
                    return new GeneralResponse<List<GetProductsDTO>>
                    {
                        Data = null,
                        Message = "No products found for the specified seller.",
                        Succeeded = true,
                        PaginationInfo = null
                    };
                }

                var paginatedList = PaginationHelper.Paginate(result, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);

                var getProductsDTOs = mapper.Map<List<GetProductsDTO>>(paginatedList.Items);

                return new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = getProductsDTOs,
                    Message = "Products retrieved successfully",
                    Succeeded = true,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {


                return new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = null,
                    Message = "An error occurred while retrieving products.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<List<GetProductsDTO>>> GetProductsByIds(List<int> productIds, int page = 1, int pageSize = 10)
        {
            try
            {
                if (productIds == null || !productIds.Any())
                {
                    return new GeneralResponse<List<GetProductsDTO>>
                    {
                        Data = new List<GetProductsDTO>(),
                        Message = "No product IDs provided.",
                        Succeeded = true,
                        PaginationInfo = null
                    };
                }

                IQueryable<Product> query = unitOfWork.productRepository.FindAll(
                     new string[] { "productImages", "Features", "Reviews", "ProductSizes" }
                );

                query = query.Where(p => productIds.Contains(p.ProductId));

                var paginatedList = PaginationHelper.Paginate(query, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);

                var getProductsDTOs = mapper.Map<List<GetProductsDTO>>(paginatedList.Items);

                return new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = getProductsDTOs,
                    Message = "Products retrieved successfully",
                    Succeeded = true,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<GetProductsDTO>>
                {
                    Data = null,
                    Message = "An error occurred while retrieving products.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<GeneralResponse<List<Product>>> GetProductsByMainCategory(
    int categoryId,
    int page = 1,
    int pageSize = 10,
    string searchTerm = "",
    decimal? minPrice = null,
    decimal? maxPrice = null,
    string sortOption = "",
             int subcategoryId = 0
            )
        {
            try
            {
                var allProducts = await unitOfWork.productRepository.FindAllByOrder(
                    new string[] { "productImages", "ProductSizes", "Features", "Reviews" },
       p => p.CategoryId == categoryId && (subcategoryId == 0 || p.SubCategoryId == subcategoryId),
            orderBy: p => p.OrderByDescending(p => p.CreatedAt)


                );

                var filteredProducts = allProducts
                    .Where(p => string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm))
                    .Where(p => !minPrice.HasValue || p.Price >= minPrice.Value)
                    .Where(p => !maxPrice.HasValue || p.Price <= maxPrice.Value);

                IQueryable<Product> sortedProducts;
                switch (sortOption.ToLower())
                {
                    case "price-low":
                        sortedProducts = filteredProducts.OrderBy(p => p.Price);
                        break;
                    case "price-high":
                        sortedProducts = filteredProducts.OrderByDescending(p => p.Price);
                        break;
                    case "rating":
                        sortedProducts = filteredProducts.OrderByDescending(p => p.Rating);
                        break;
                    default:
                        sortedProducts = filteredProducts.OrderBy(p => p.Name);
                        break;
                }

                var paginatedList = PaginationHelper.Paginate(sortedProducts, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);

                return new GeneralResponse<List<Product>>
                {
                    Data = paginatedList.Items,
                    Message = "Products retrieved successfully",
                    Succeeded = true,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<Product>>
                {
                    Data = null,
                    Message = "An error occurred while retrieving products.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<List<Product>>> GetProductsBySubCategory(
   int subCategoryId,
   int page = 1,
   int pageSize = 10,
   string searchTerm = "",
   decimal? minPrice = null,
   decimal? maxPrice = null,
   string sortOption = ""
           )
        {
            try
            {
                var allProducts = await unitOfWork.productRepository.FindAllByOrder(
                    new string[] { "productImages", "ProductSizes", "Features", "Reviews" },
       p => p.SubCategoryId == subCategoryId,
            orderBy: p => p.OrderByDescending(p => p.CreatedAt)


                );

                var filteredProducts = allProducts
                    .Where(p => string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm))
                    .Where(p => !minPrice.HasValue || p.Price >= minPrice.Value)
                    .Where(p => !maxPrice.HasValue || p.Price <= maxPrice.Value);

                IQueryable<Product> sortedProducts;
                switch (sortOption.ToLower())
                {
                    case "price-low":
                        sortedProducts = filteredProducts.OrderBy(p => p.Price);
                        break;
                    case "price-high":
                        sortedProducts = filteredProducts.OrderByDescending(p => p.Price);
                        break;
                    case "rating":
                        sortedProducts = filteredProducts.OrderByDescending(p => p.Rating);
                        break;
                    default:
                        sortedProducts = filteredProducts.OrderBy(p => p.Name);
                        break;
                }

                var paginatedList = PaginationHelper.Paginate(sortedProducts, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);

                return new GeneralResponse<List<Product>>
                {
                    Data = paginatedList.Items,
                    Message = "Products retrieved successfully",
                    Succeeded = true,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<Product>>
                {
                    Data = null,
                    Message = "An error occurred while retrieving products.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }



        public async Task<GeneralResponse<List<Product>>> GetSearchProduct(
  int page = 1,
  int pageSize = 10,
  string searchTerm = "",
  decimal? minPrice = null,
  decimal? maxPrice = null,
  string sortOption = ""
          )
        {
            try
            {
                var allProducts = await unitOfWork.productRepository.FindAllByOrder(
                    new string[] { "productImages", "ProductSizes", "Features", "Reviews" },
            orderBy: p => p.OrderByDescending(p => p.CreatedAt)


                );

                var filteredProducts = allProducts
                    .Where(p => string.IsNullOrEmpty(searchTerm) || p.Name.Contains(searchTerm))
                    .Where(p => !minPrice.HasValue || p.Price >= minPrice.Value)
                    .Where(p => !maxPrice.HasValue || p.Price <= maxPrice.Value);

                IQueryable<Product> sortedProducts;
                switch (sortOption.ToLower())
                {
                    case "price-low":
                        sortedProducts = filteredProducts.OrderBy(p => p.Price);
                        break;
                    case "price-high":
                        sortedProducts = filteredProducts.OrderByDescending(p => p.Price);
                        break;
                    case "rating":
                        sortedProducts = filteredProducts.OrderByDescending(p => p.Rating);
                        break;
                    default:
                        sortedProducts = filteredProducts.OrderBy(p => p.Name);
                        break;
                }

                var paginatedList = PaginationHelper.Paginate(sortedProducts, page, pageSize);
                var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);

                return new GeneralResponse<List<Product>>
                {
                    Data = paginatedList.Items,
                    Message = "Products retrieved successfully",
                    Succeeded = true,
                    PaginationInfo = paginationInfo
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<List<Product>>
                {
                    Data = null,
                    Message = "An error occurred while retrieving products.",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<ProductDTO>> EditProduct(EditProductDTO editProductDTO, int productId)
        {
            using (var transaction = await unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var existingProduct = await unitOfWork.productRepository.GetByIdAsync(productId);
                    if (existingProduct == null)
                    {
                        return new GeneralResponse<ProductDTO>
                        {
                            Data = null,
                            Message = "Product not found",
                            Succeeded = false,
                            Errors = new List<string> { "Product does not exist" }
                        };
                    }

                    existingProduct.Name = editProductDTO.Name;
                    existingProduct.Description = editProductDTO.Description;
                    existingProduct.Price = editProductDTO.Price;
                    existingProduct.CategoryId = editProductDTO.CategoryId;
                    existingProduct.SubCategoryId = editProductDTO.SubCategoryId;
                    existingProduct.StockQuantity = editProductDTO.StockQuantity ?? existingProduct.StockQuantity;
                    existingProduct.Discount = editProductDTO.Discount;
                    existingProduct.IsFreeShipping = editProductDTO.IsFreeShipping;
                    existingProduct.DeliveryFee = editProductDTO.DeliveryFee;
                    if (editProductDTO.productImages != null && editProductDTO.productImages.Any())
                    {
                        var newImageUrls = await ImageSavingHelper.SaveImagesAsync(editProductDTO.productImages, "ProductImages");

                        var oldImages = existingProduct.productImages.Where(img => img.productId == existingProduct.ProductId).ToList();

                        var newImages = newImageUrls.Select(url => new ProductImages
                        {
                            productId = existingProduct.ProductId,
                            ImageUrl = url
                        }).ToList();

                        existingProduct.productImages.AddRange(newImages);
                    }

                    if (editProductDTO.productImagesUrl != null && editProductDTO.productImagesUrl.Any())
                    {
                        existingProduct.productImages = editProductDTO.productImagesUrl.Select(url => new ProductImages { ImageUrl = url }).ToList();
                    }

                    if (!string.IsNullOrEmpty(editProductDTO.productSizesJson))
                    {
                        var productSizes = JsonConvert.DeserializeObject<List<ProductSizeDTO>>(editProductDTO.productSizesJson);

                        if (productSizes != null && productSizes.Any())
                        {
                            existingProduct.ProductSizes.Clear();
                            existingProduct.ProductSizes.AddRange(mapper.Map<List<ProductSize>>(productSizes));
                        }
                    }

                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var res = mapper.Map<ProductDTO>(existingProduct);
                    return new GeneralResponse<ProductDTO>
                    {
                        Data = res,
                        Message = "Product updated successfully",
                        Succeeded = true,
                        Errors = null
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse<ProductDTO>
                    {
                        Data = null,
                        Message = "Unexpected error occurred",
                        Succeeded = false,
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            // Ensure the images directory exists
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "ProductImages");
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
            }

            // Generate a unique file name to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{image.FileName}";
            var filePath = Path.Combine(imagesPath, uniqueFileName);

            // Save the image to the file system
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Return the relative path to the image
            var imageUrl = $"/images/{uniqueFileName}";
            return imageUrl;
        }


        public async Task<GeneralResponse<List<int>>> GetProductImageIds(int productId)
        {
            try
            {
                // Fetch all entities matching the criteria
                var productImages = await unitOfWork.GetRepository<ProductImages>()
                                                    .FindAllAsync(criteria: p => p.productId == productId);

                // Select the ImageId from the results
                var imageIds = productImages.Select(p => p.Id).ToList();

                // Create a success response
                return new GeneralResponse<List<int>>
                {
                    Data = imageIds,
                    Message = "Image IDs retrieved successfully.",
                    Succeeded = true,
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                // Log error and create an error response
                return new GeneralResponse<List<int>>
                {
                    Data = null,
                    Message = $"Error retrieving image IDs for product {productId}: {ex.Message}",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<bool>> DeleteProduct(int productId)
        {
            try
            {
                // Fetch the product entity along with related entities from the database
                var product = await unitOfWork.productRepository.FindAsync(
                    criteria: p => p.ProductId == productId,
                    includes: new string[] { "productImages", "Features", "ProductSizes" }
                );

                if (product != null)
                {
                    // Delete associated images
                    if (product.productImages.Any())
                    {
                        unitOfWork.GetRepository<ProductImages>().DeleteRange(product.productImages);
                    }

                    // Delete associated features
                    if (product.Features.Any())
                    {
                        unitOfWork.GetRepository<Feature>().DeleteRange(product.Features);
                    }

                    // Delete associated sizes
                    if (product.ProductSizes.Any())
                    {
                        unitOfWork.GetRepository<ProductSize>().DeleteRange(product.ProductSizes);
                    }

                    // Delete the product itself using the productId
                    await unitOfWork.productRepository.DeleteAsync(productId);
                    await unitOfWork.SaveChangesAsync();

                    return new GeneralResponse<bool>
                    {
                        Data = true,
                        Message = "Product deleted successfully.",
                        Succeeded = true,
                        Errors = null
                    };
                }

                return new GeneralResponse<bool>
                {
                    Data = false,
                    Message = "Product not found.",
                    Succeeded = false,
                    Errors = null
                };
            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return new GeneralResponse<bool>
                {
                    Data = false,
                    Message = $"Error while deleting product: {innerExceptionMessage}",
                    Succeeded = false,
                    Errors = new List<string> { innerExceptionMessage }
                };
            }
            catch (Exception ex)
            {
                return new GeneralResponse<bool>
                {
                    Data = false,
                    Message = $"Unexpected error occurred: {ex.Message}",
                    Succeeded = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GeneralResponse<List<Product>>> GetRecommendedProductsAsync()
        {

            var recommendedProducts = await unitOfWork.productRepository.FindAllAsync(
                 new string[] { "productImages", "Features", "ProductSizes" },
                  p => p.Rating >= 4.5 || p.NumberOfRatings > 10);

            var top10Products = recommendedProducts
                       .OrderByDescending(p => p.PurchaseNumber)
                       .Take(10)
                       .ToList();

            var response = new GeneralResponse<List<Product>>
            {
                Data = top10Products.ToList(),
                Succeeded = true,
                Message = top10Products.Any() ? "Recommended products retrieved successfully." : "No recommended products found.",
                Errors = null // No errors
            };

            return response;
        }

        public async Task<GeneralResponse<List<Product>>> GetDealProductsAsync()
        {
            var dealsProducts = await unitOfWork.productRepository.FindAllAsync(
                 new string[] { "productImages", "Features", "ProductSizes" },
                  p => p.Discount != null);

            var top10Products = dealsProducts
                        .OrderByDescending(p => p.PurchaseNumber)
                        .Take(10)
                        .ToList();

            var response = new GeneralResponse<List<Product>>
            {
                Data = top10Products,
                Succeeded = true,
                Message = top10Products.Any() ? "Deals products retrieved successfully." : "No Deals products found.",
                Errors = null // No errors
            };

            return response;
        }

        public async Task<GeneralResponse<List<Product>>> GetBestSellerProductsAsync()
        {
            var bestSellerProducts = await unitOfWork.productRepository.FindAllAsync(
                new string[] { "productImages", "Features", "ProductSizes" },
                 p => p.PurchaseNumber != null);

            var top10Products = bestSellerProducts
                        .OrderByDescending(p => p.PurchaseNumber)
                        .Take(10)
                        .ToList();


            var response = new GeneralResponse<List<Product>>
            {
                Data = top10Products,
                Succeeded = true,
                Message = top10Products.Any() ? "Best Seller products retrieved successfully." : "No Best Seller products found.",
                Errors = null // No errors
            };

            return response;
        }


        public async Task<GeneralResponse<List<Product>>> GetRandomProductsAsync()
        {
            // Fetch all products including related entities
            var allProducts = await unitOfWork.productRepository.FindAllAsync(
                new string[] { "productImages", "Features", "ProductSizes" });

            // Select 10 random products
            var randomProducts = allProducts
                .OrderBy(x => Guid.NewGuid()) // Randomize the order
                .Take(10) // Take the top 10
                .ToList();

            // Prepare the response
            var response = new GeneralResponse<List<Product>>
            {
                Data = randomProducts,
                Succeeded = true,
                Message = randomProducts.Any() ? "Random products retrieved successfully." : "No products found.",
                Errors = null // No errors
            };

            return response;
        }


        public async Task<GeneralResponse<List<Product>>> GetMoreFromSellerProductsAsync(string sellerId)
        {
            var allProducts = await unitOfWork.productRepository.FindAllAsync(
                new string[] { "productImages", "Features", "ProductSizes" }, p => p.sellerId == sellerId);

            var randomProducts = allProducts
                .OrderBy(x => Guid.NewGuid())
                .Take(10)
                .ToList();

            var response = new GeneralResponse<List<Product>>
            {
                Data = randomProducts,
                Succeeded = true,
                Message = randomProducts.Any() ? "more from seller products retrieved successfully." : "No products found.",
                Errors = null
            };

            return response;
        }

        public async Task<GeneralResponse<List<Product>>> GetMoreFromCategoryProductsAsync(int CategoryId)
        {
            var allProducts = await unitOfWork.productRepository.FindAllAsync(
                new string[] { "productImages", "Features", "ProductSizes" }, p => p.CategoryId == CategoryId);

            var randomProducts = allProducts
                .OrderBy(x => Guid.NewGuid())
                .Take(10)
                .ToList();

            var response = new GeneralResponse<List<Product>>
            {
                Data = randomProducts,
                Succeeded = true,
                Message = randomProducts.Any() ? "more from category products retrieved successfully." : "No products found.",
                Errors = null
            };

            return response;
        }
    }
}
