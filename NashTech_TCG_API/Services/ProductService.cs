using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Utilities;

namespace NashTech_TCG_API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IdGenerator _idGenerator;
        private readonly ILogger<ProductService> _logger;
        private readonly IFirebaseStorageService _firebaseStorage;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IdGenerator idGenerator,
            ILogger<ProductService> logger,
            IFirebaseStorageService firebaseStorage)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
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
            bool ascending = true)
        {
            var (products, totalCount) = await _productRepository.GetPagedProductsAsync(
                pageNumber, pageSize, categoryId, searchTerm, sortBy, ascending);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var productDTOs = await Task.WhenAll(products.Select(async p => await MapToDTO(p)));

            return (productDTOs, totalCount, totalPages);
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

            return new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };
        }
    }
}
