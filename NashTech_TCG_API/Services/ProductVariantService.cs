using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Utilities;
using NashTech_TCG_API.Utilities.Interfaces;
using static NashTech_TCG_API.Models.DTOs.ProductVariantDTOs;

namespace NashTech_TCG_API.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRarityRepository _rarityRepository;
        private readonly IIdGenerator _idGenerator;
        private readonly ILogger<ProductVariantService> _logger;
        private readonly IFirebaseStorageService _firebaseStorage;

        public ProductVariantService(
            IProductVariantRepository productVariantRepository,
            IProductRepository productRepository,
            IRarityRepository rarityRepository,
            IIdGenerator idGenerator,
            ILogger<ProductVariantService> logger,
            IFirebaseStorageService firebaseStorage)
        {
            _productVariantRepository = productVariantRepository;
            _productRepository = productRepository;
            _rarityRepository = rarityRepository;
            _idGenerator = idGenerator;
            _logger = logger;
            _firebaseStorage = firebaseStorage;
        }

        public async Task<ProductVariantDTO> GetByIdAsync(string id)
        {
            var productVariant = await _productVariantRepository.GetByIdAsync(id);
            return productVariant != null ? MapToDTO(productVariant) : null;
        }

        public async Task<IEnumerable<ProductVariantDTO>> GetAllAsync()
        {
            var productVariants = await _productVariantRepository.GetAllAsync();
            return productVariants.Select(pv => MapToDTO(pv));
        }

        public async Task<(IEnumerable<ProductVariantDTO> ProductVariants, int TotalCount, int TotalPages)> GetPagedProductVariantsAsync(
            int pageNumber,
            int pageSize,
            string productId = null,
            string rarityId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            var (productVariants, totalCount) = await _productVariantRepository.GetPagedProductVariantsAsync(
                pageNumber, pageSize, productId, rarityId, minPrice, maxPrice, searchTerm, sortBy, ascending);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var productVariantDTOs = productVariants.Select(pv => MapToDTO(pv));

            return (productVariantDTOs, totalCount, totalPages);
        }

        public async Task<ProductVariantDTO> CreateProductVariantAsync(CreateProductVariantDTO productVariantDTO)
        {
            try
            {
                // Validate if product exists
                var product = await _productRepository.GetByIdAsync(productVariantDTO.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: {productVariantDTO.ProductId}");
                    return null;
                }

                // Validate if rarity exists
                var rarity = await _rarityRepository.GetByIdAsync(productVariantDTO.RarityId);
                if (rarity == null)
                {
                    _logger.LogWarning($"Rarity not found: {productVariantDTO.RarityId}");
                    return null;
                }

                // Check if a variant with this product and rarity already exists
                var existingVariant = await _productVariantRepository.GetByProductAndRarityAsync(
                    productVariantDTO.ProductId, productVariantDTO.RarityId);

                if (existingVariant != null)
                {
                    _logger.LogWarning($"A variant for product {productVariantDTO.ProductId} with rarity {productVariantDTO.RarityId} already exists");
                    return null;
                }

                // Upload image to Firebase if provided
                string imageUrl = null;
                if (productVariantDTO.ImageFile != null && productVariantDTO.ImageFile.Length > 0)
                {
                    imageUrl = await _firebaseStorage.UploadFileAsync(productVariantDTO.ImageFile, "variants");
                    _logger.LogInformation($"Image uploaded to Firebase: {imageUrl}");
                }

                // Generate a prefixed ID for the product variant (VAR001, VAR002, etc.)
                string variantId = await _idGenerator.GenerateId("VAR");

                var productVariant = new ProductVariant
                {
                    VariantId = variantId,
                    ProductId = productVariantDTO.ProductId,
                    RarityId = productVariantDTO.RarityId,
                    Price = productVariantDTO.Price,
                    StockQuantity = productVariantDTO.StockQuantity,
                    ImageUrl = imageUrl
                };

                await _productVariantRepository.AddAsync(productVariant);
                await _productVariantRepository.SaveChangesAsync();

                // Load related entities for the DTO
                productVariant = await _productVariantRepository.GetByIdAsync(variantId);

                _logger.LogInformation($"Created new product variant with ID: {variantId}");
                return MapToDTO(productVariant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product variant");
                throw;
            }
        }

        public async Task<ProductVariantDTO> UpdateProductVariantAsync(string id, UpdateProductVariantDTO productVariantDTO)
        {
            var productVariant = await _productVariantRepository.GetByIdAsync(id);
            if (productVariant == null)
            {
                _logger.LogWarning($"Product variant not found: {id}");
                return null;
            }

            // If a new image is uploaded, delete the old one and upload the new one
            if (productVariantDTO.ImageFile != null && productVariantDTO.ImageFile.Length > 0)
            {
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(productVariant.ImageUrl))
                {
                    await _firebaseStorage.DeleteFileAsync(productVariant.ImageUrl);
                }

                // Upload new image
                string newImageUrl = await _firebaseStorage.UploadFileAsync(productVariantDTO.ImageFile, "variants");
                productVariant.ImageUrl = newImageUrl;
                _logger.LogInformation($"Image updated for product variant {id}. New URL: {newImageUrl}");
            }

            productVariant.Price = productVariantDTO.Price;
            productVariant.StockQuantity = productVariantDTO.StockQuantity;

            await _productVariantRepository.UpdateAsync(productVariant);
            await _productVariantRepository.SaveChangesAsync();

            // Load related entities for the DTO
            productVariant = await _productVariantRepository.GetByIdAsync(id);

            _logger.LogInformation($"Updated product variant: {id}");
            return MapToDTO(productVariant);
        }

        public async Task<bool> DeleteProductVariantAsync(string id)
        {
            var productVariant = await _productVariantRepository.GetByIdAsync(id);
            if (productVariant == null)
            {
                _logger.LogWarning($"Product variant not found: {id}");
                return false;
            }

            // Delete the variant image from Firebase if it exists
            if (!string.IsNullOrEmpty(productVariant.ImageUrl))
            {
                await _firebaseStorage.DeleteFileAsync(productVariant.ImageUrl);
                _logger.LogInformation($"Deleted image for product variant {id}");
            }

            await _productVariantRepository.RemoveAsync(productVariant);
            await _productVariantRepository.SaveChangesAsync();

            _logger.LogInformation($"Deleted product variant: {id}");
            return true;
        }

        private ProductVariantDTO MapToDTO(ProductVariant productVariant)
        {
            return new ProductVariantDTO
            {
                VariantId = productVariant.VariantId,
                ProductId = productVariant.ProductId,
                ProductName = productVariant.Product?.Name,
                RarityId = productVariant.RarityId,
                RarityName = productVariant.Rarity?.Name,
                Price = productVariant.Price,
                StockQuantity = productVariant.StockQuantity,
                ImageUrl = productVariant.ImageUrl
            };
        }
    }
}
