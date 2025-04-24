using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Utilities;
using NashTech_TCG_ShareViewModels.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace NashTech_TCG_API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IRarityRepository _rarityRepository; // Add this
        private readonly IProductRatingRepository _productRatingRepository; // Add this
        private readonly IdGenerator _idGenerator;
        private readonly ILogger<ProductService> _logger;
        private readonly IFirebaseStorageService _firebaseStorage;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductVariantRepository productVariantRepository,
            IRarityRepository rarityRepository, // Add this
            IProductRatingRepository productRatingRepository, // Add this
            IdGenerator idGenerator,
            ILogger<ProductService> logger,
            IFirebaseStorageService firebaseStorage)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productVariantRepository = productVariantRepository;
            _rarityRepository = rarityRepository; // Initialize this
            _productRatingRepository = productRatingRepository; // Initialize this
            _idGenerator = idGenerator;
            _logger = logger;
            _firebaseStorage = firebaseStorage;
        }

        public async Task<ProductDTO> GetByIdAsync(string id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? await MapToDTO(product) : null;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return await Task.WhenAll(products.Select(async p => await MapToDTO(p)));
        }

        public async Task<(IEnumerable<ProductDTO> Products, int TotalCount, int TotalPages)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            string categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true,
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            try
            {
                // Get base products with pagination and filtering
                var (products, totalCount) = await _productRepository.GetPagedProductsAsync(
                    pageNumber, pageSize, categoryId, searchTerm, sortBy, ascending);

                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Transform products to DTOs with price info
                var productDTOs = new List<ProductDTO>();

                foreach (var product in products)
                {
                    // Ensure category is loaded
                    if (product.Category == null)
                    {
                        product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                    }

                    // Get all variants for this product to calculate price range
                    var variants = await _productVariantRepository.GetVariantsByProductIdAsync(product.ProductId);
                    // Get all variants for this product to calculate price range
                   

                    // Only calculate min/max if variants exist
                    if (variants.Any())
                    {
                        minPrice = variants.Min(v => v.Price);
                        maxPrice = variants.Max(v => v.Price);
                    }

                    // Apply price filter if set
                    if ((minPrice.HasValue || maxPrice.HasValue) && variants.Any())
                    {
                        var productMinPrice = variants.Min(v => v.Price);
                        var productMaxPrice = variants.Max(v => v.Price);

                        // Skip this product if it doesn't match the price filter
                        if ((minPrice.HasValue && productMaxPrice < minPrice) ||
                            (maxPrice.HasValue && productMinPrice > maxPrice))
                        {
                            continue;
                        }
                    }

                    var dto = new ProductDTO
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category?.Name,
                        Description = product.Description,
                        ImageUrl = product.ImageUrl,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        CreatedDate = product.CreatedDate,
                        UpdatedDate = product.UpdatedDate
                    };

                    productDTOs.Add(dto);
                }

                return (productDTOs, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged products");
                throw;
            }
        }


        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO)
        {
            try
            {
                // Validate if category exists
                var category = await _categoryRepository.GetByIdAsync(productDTO.CategoryId);
                if (category == null)
                {
                    _logger.LogWarning($"Category not found: {productDTO.CategoryId}");
                    return null;
                }

                // Upload image to Firebase if provided
                string imageUrl = null;
                if (productDTO.ImageFile != null && productDTO.ImageFile.Length > 0)
                {
                    imageUrl = await _firebaseStorage.UploadFileAsync(productDTO.ImageFile, "products");
                    _logger.LogInformation($"Image uploaded to Firebase: {imageUrl}");
                }

                // Generate a prefixed ID for the product (PROD001, PROD002, etc.)
                string productId = await _idGenerator.GenerateId("PROD");

                var product = new Product
                {
                    ProductId = productId,
                    Name = productDTO.Name,
                    CategoryId = productDTO.CategoryId,
                    Description = productDTO.Description,
                    ImageUrl = imageUrl,
                    CreatedDate = DateTime.UtcNow
                };

                await _productRepository.AddAsync(product);
                await _productRepository.SaveChangesAsync();

                _logger.LogInformation($"Created new product with ID: {productId}");
                return await MapToDTO(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<ProductDTO> UpdateProductAsync(string id, UpdateProductDTO productDTO)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product not found: {id}");
                return null;
            }

            // Validate if category exists
            var category = await _categoryRepository.GetByIdAsync(productDTO.CategoryId);
            if (category == null)
            {
                _logger.LogWarning($"Category not found: {productDTO.CategoryId}");
                return null;
            }

            // If a new image is uploaded, delete the old one and upload the new one
            if (productDTO.ImageFile != null && productDTO.ImageFile.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _firebaseStorage.DeleteFileAsync(product.ImageUrl);
                }

                // Upload new image
                string newImageUrl = await _firebaseStorage.UploadFileAsync(productDTO.ImageFile, "products");
                product.ImageUrl = newImageUrl;
                _logger.LogInformation($"Image updated for product {id}. New URL: {newImageUrl}");
            }
            //// If no new image but a URL is provided in the DTO, use that
            //else if (!string.IsNullOrEmpty(productDTO.ImageUrl))
            //{
            //    product.ImageUrl = productDTO.ImageUrl;
            //}
            //// Otherwise keep the existing image

            product.Name = productDTO.Name;
            product.CategoryId = productDTO.CategoryId;
            product.Description = productDTO.Description;
            product.UpdatedDate = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation($"Updated product: {id}");
            return await MapToDTO(product);
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product not found: {id}");
                return false;
            }

            // Delete the product image from Firebase if it exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                await _firebaseStorage.DeleteFileAsync(product.ImageUrl);
                _logger.LogInformation($"Deleted image for product {id}");
            }

            await _productRepository.RemoveAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation($"Deleted product: {id}");
            return true;
        }

        private async Task<ProductDTO> MapToDTO(Product product)
        {
            // Ensure category is loaded
            if (product.Category == null)
            {
                product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            }

            // Get all variants for this product to calculate price range
            var variants = await _productVariantRepository.GetVariantsByProductIdAsync(product.ProductId);

            decimal? minPrice = null;
            decimal? maxPrice = null;

            // Only calculate min/max if variants exist
            if (variants.Any())
            {
                minPrice = variants.Min(v => v.Price);
                maxPrice = variants.Max(v => v.Price);
            }

            return new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };
        }


        public async Task<(IEnumerable<ProductViewModel> Products, int TotalCount, int TotalPages)> GetPagedProductsForClientAsync(
            int pageNumber,
            int pageSize,
            string categoryId = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            try
            {
                // Get base products with pagination and filtering
                var (products, totalCount) = await _productRepository.GetPagedProductsAsync(
                    pageNumber, pageSize, categoryId, searchTerm, sortBy, ascending);

                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Transform products to view models and load price information
                var productViewModels = new List<ProductViewModel>();

                foreach (var product in products)
                {
                    // Ensure category is loaded
                    if (product.Category == null)
                    {
                        product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                    }

                    // Get all variants for this product to calculate price range
                    var variants = await _productVariantRepository.GetVariantsByProductIdAsync(product.ProductId);

                    decimal? minPrice = null;
                    decimal? maxPrice = null;

                    // Only calculate min/max if variants exist
                    if (variants.Any())
                    {
                        minPrice = variants.Min(v => v.Price);
                        maxPrice = variants.Max(v => v.Price);
                    }

                    var viewModel = new ProductViewModel
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category?.Name,
                        Description = product.Description,
                        ImageUrl = product.ImageUrl,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        CreatedDate = product.CreatedDate
                    };

                    productViewModels.Add(viewModel);
                }

                _logger.LogInformation($"Retrieved {productViewModels.Count} products for client display");
                return (productViewModels, totalCount, totalPages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for client");
                throw;
            }
        }

        // Update the GetProductDetailsAsync method
        public async Task<ProductViewModel> GetProductDetailsAsync(string id)
        {
            try
            {
                // Get the product
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: {id}");
                    return null;
                }

                // Ensure category is loaded
                if (product.Category == null)
                {
                    product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                }

                // Get all variants for this product
                var variants = await _productVariantRepository.GetVariantsByProductIdAsync(product.ProductId);

                // Get all ratings for this product
                var ratings = await _productRatingRepository.GetRatingsByProductIdAsync(id);

                // Calculate average rating
                double averageRating = 0;
                if (ratings.Any())
                {
                    averageRating = ratings.Average(r => r.Rating);
                }

                // Calculate min/max price from variants
                decimal? minPrice = null;
                decimal? maxPrice = null;

                // Only calculate min/max if variants exist
                if (variants.Any())
                {
                    minPrice = variants.Min(v => v.Price);
                    maxPrice = variants.Max(v => v.Price);
                }

                // Map variants to view model
                var variantViewModels = new List<ProductVariantViewModel>();
                foreach (var variant in variants)
                {
                    // Ensure rarity is loaded
                    if (variant.Rarity == null)
                    {
                        variant.Rarity = await _rarityRepository.GetByIdAsync(variant.RarityId);
                    }

                    variantViewModels.Add(new ProductVariantViewModel
                    {
                        VariantId = variant.VariantId,
                        ProductId = variant.ProductId,
                        RarityId = variant.RarityId,
                        RarityName = variant.Rarity?.Name ?? "Unknown",
                        Price = variant.Price,
                        StockQuantity = variant.StockQuantity,
                        ImageUrl = variant.ImageUrl
                    });
                }

                // Map ratings to view model
                var ratingViewModels = ratings.Select(r => new ProductRatingViewModel
                {
                    RatingId = r.RatingId,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = $"{r.ApplicationUser.FirstName} {r.ApplicationUser.LastName}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedDate = r.CreatedDate
                }).ToList();

                // Create the final view model
                var viewModel = new ProductViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    CreatedDate = product.CreatedDate,
                    Variants = variantViewModels,
                    Ratings = ratingViewModels,
                    AverageRating = Math.Round(averageRating, 1),
                    RatingCount = ratings.Count()
                };

                _logger.LogInformation($"Retrieved product details for ID: {id}");
                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product details for ID: {id}");
                throw;
            }
        }

        public async Task<ProductRatingViewModel> AddProductRatingAsync(ProductRatingInputViewModel model, string userId)
        {
            try
            {
                // Validate that product exists
                var product = await _productRepository.GetByIdAsync(model.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: {model.ProductId}");
                    return null;
                }

                // Create new rating entity
                var rating = new ProductRating
                {
                    ProductId = model.ProductId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedDate = DateTime.UtcNow
                };

                // Add the rating via repository
                await _productRatingRepository.AddAsync(rating);
                await _productRatingRepository.SaveChangesAsync();

                // Get user information
                // Since we don't have a direct user repository, we'll need to get the user from the rating
                var ratingWithUser = (await _productRatingRepository.GetRatingsByProductIdAsync(model.ProductId))
                    .FirstOrDefault(r => r.RatingId == rating.RatingId);

                if (ratingWithUser == null || ratingWithUser.ApplicationUser == null)
                {
                    _logger.LogWarning($"Failed to load user information for rating {rating.RatingId}");
                }
                
                var ratingViewModel = new ProductRatingViewModel
                {
                    RatingId = rating.RatingId,
                    ProductId = rating.ProductId,
                    UserId = rating.UserId,
                    UserName = ratingWithUser?.ApplicationUser != null ?
                $"{ratingWithUser.ApplicationUser.FirstName} {ratingWithUser.ApplicationUser.LastName}" :
                "Anonymous",
                    Rating = rating.Rating,
                    Comment = rating.Comment,
                    CreatedDate = rating.CreatedDate
                };

                _logger.LogInformation($"Added new rating for product {model.ProductId} by user {userId}");
                return ratingViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding rating for product {model.ProductId} by user {userId}");
                throw;
            }
        }

        // Update in ProductService.cs
        public async Task<IEnumerable<ProductViewModel>> GetTopRatedProductsByCategoryAsync(string categoryId, int limit = 8)
        {
            try
            {
                // Validate the category exists
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category == null)
                {
                    _logger.LogWarning($"Category not found: {categoryId}");
                    return Enumerable.Empty<ProductViewModel>();
                }

                // Get top rated products from repository for this specific category
                var products = await _productRepository.GetTopRatedProductsByCategoryAsync(categoryId, limit);

                // Transform to view models
                var productViewModels = new List<ProductViewModel>();

                foreach (var product in products)
                {
                    // Get variants for price calculation
                    var variants = await _productVariantRepository.GetVariantsByProductIdAsync(product.ProductId);

                    // Calculate min/max price
                    decimal? minPrice = null;
                    decimal? maxPrice = null;
                    if (variants.Any())
                    {
                        minPrice = variants.Min(v => v.Price);
                        maxPrice = variants.Max(v => v.Price);
                    }

                    // Calculate average rating
                    double averageRating = 0;
                    if (product.ProductRatings.Any())
                    {
                        averageRating = product.ProductRatings.Average(r => r.Rating);
                    }

                    var viewModel = new ProductViewModel
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        CategoryId = product.CategoryId,
                        CategoryName = category.Name,
                        Description = product.Description,
                        ImageUrl = product.ImageUrl,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        CreatedDate = product.CreatedDate,
                        AverageRating = Math.Round(averageRating, 1),
                        RatingCount = product.ProductRatings.Count
                    };

                    productViewModels.Add(viewModel);
                }

                _logger.LogInformation($"Retrieved {productViewModels.Count} top rated products for category {categoryId}");
                return productViewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving top rated products for category {categoryId}");
                throw;
            }
        }





    }
}
