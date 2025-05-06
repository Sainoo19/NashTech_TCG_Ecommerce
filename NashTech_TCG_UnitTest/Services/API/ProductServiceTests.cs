using Microsoft.Extensions.Logging;
using Moq;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Utilities;
using NashTech_TCG_API.Utilities.Interfaces;
using NashTech_TCG_ShareViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NashTech_TCG_UnitTest.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductVariantRepository> _mockProductVariantRepository;
        private readonly Mock<IRarityRepository> _mockRarityRepository;
        private readonly Mock<IProductRatingRepository> _mockProductRatingRepository;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IIdGenerator> _mockIdGenerator; 
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly Mock<IFirebaseStorageService> _mockFirebaseStorage;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Initialize mocks
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductVariantRepository = new Mock<IProductVariantRepository>();
            _mockRarityRepository = new Mock<IRarityRepository>();
            _mockProductRatingRepository = new Mock<IProductRatingRepository>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockIdGenerator = new Mock<IIdGenerator>(); 
            _mockLogger = new Mock<ILogger<ProductService>>();
            _mockFirebaseStorage = new Mock<IFirebaseStorageService>();

            // Create the service with mocked dependencies
            _productService = new ProductService(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object,
                _mockProductVariantRepository.Object,
                _mockRarityRepository.Object,
                _mockProductRatingRepository.Object,
                _mockOrderRepository.Object,
                _mockIdGenerator.Object,
                _mockLogger.Object,
                _mockFirebaseStorage.Object
            );
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            string productId = "PROD001";
            var product = new Product
            {
                ProductId = productId,
                Name = "Test Product",
                CategoryId = "CAT001",
                Description = "Test Description",
                CreatedDate = DateTime.UtcNow
            };

            var category = new Category { Name = "Test Category" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(product.CategoryId))
                .ReturnsAsync(category);
            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(productId))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(category.Name, result.CategoryName);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            string productId = "INVALID_ID";
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = "PROD001", Name = "Product 1", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow },
                new Product { ProductId = "PROD002", Name = "Product 2", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow }
            };

            var category = new Category { Name = "Test Category" };

            _mockProductRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(products);
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(category);
            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var result = await _productService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(products.Count, result.Count());
        }

        #endregion

        #region GetPagedProductsAsync Tests

        [Fact]
        public async Task GetPagedProductsAsync_ReturnsPagedProducts()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            int totalCount = 15;

            var products = new List<Product>
            {
                new Product { ProductId = "PROD001", Name = "Product 1", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow },
                new Product { ProductId = "PROD002", Name = "Product 2", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow }
            };

            var category = new Category { Name = "Test Category" };

            _mockProductRepository.Setup(repo => repo.GetPagedProductsAsync(
                pageNumber, pageSize, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true))
                .ReturnsAsync((products, totalCount));

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(category);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var (resultProducts, resultTotalCount, resultTotalPages) = await _productService.GetPagedProductsAsync(
                pageNumber, pageSize, null, null, null, true);

            // Assert
            Assert.NotNull(resultProducts);
            Assert.Equal(products.Count, resultProducts.Count());
            Assert.Equal(totalCount, resultTotalCount);
            Assert.Equal((int)Math.Ceiling(totalCount / (double)pageSize), resultTotalPages);
        }

        [Fact]
        public async Task GetPagedProductsAsync_WithPriceFilter_FiltersProducts()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            int totalCount = 2;
            decimal minPrice = 10;
            decimal maxPrice = 50;

            var products = new List<Product>
            {
                new Product { ProductId = "PROD001", Name = "Product 1", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow },
                new Product { ProductId = "PROD002", Name = "Product 2", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow }
            };

            var category = new Category { Name = "Test Category" };

            // Create variants with different price ranges
            var variants1 = new List<ProductVariant>
            {
                new ProductVariant { VariantId = "VAR001", ProductId = "PROD001", Price = 15.99m }
            };

            var variants2 = new List<ProductVariant>
            {
                new ProductVariant { VariantId = "VAR002", ProductId = "PROD002", Price = 75.99m }
            };

            _mockProductRepository.Setup(repo => repo.GetPagedProductsAsync(
                pageNumber, pageSize, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true))
                .ReturnsAsync((products, totalCount));

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(category);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync("PROD001"))
                .ReturnsAsync(variants1);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync("PROD002"))
                .ReturnsAsync(variants2);

            // Act
            var (resultProducts, resultTotalCount, resultTotalPages) = await _productService.GetPagedProductsAsync(
                pageNumber, pageSize, null, null, null, true, minPrice, maxPrice);

            // Assert
            Assert.NotNull(resultProducts);
            Assert.Single(resultProducts); // Only one product should be in the price range
            Assert.Equal("PROD001", resultProducts.First().ProductId); // First product should match price filter
        }

        #endregion

        #region CreateProductAsync Tests

        // Update the test method
        [Fact]
        public async Task CreateProductAsync_WithValidData_CreatesProduct()
        {
            // Arrange
            var createDto = new CreateProductDTO
            {
                Name = "New Product",
                CategoryId = "CAT001",
                Description = "New Description"
            };

            var category = new Category { Name = "Test Category" };
            string generatedId = "PROD001";

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(createDto.CategoryId))
                .ReturnsAsync(category);

            // Setup the mock for IIdGenerator
            _mockIdGenerator.Setup(gen => gen.GenerateId("PROD"))
                .ReturnsAsync(generatedId);

            Product savedProduct = null;

            _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .Callback<Product>(p => savedProduct = p)
                .Returns(Task.CompletedTask);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var result = await _productService.CreateProductAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(generatedId, result.ProductId);
            Assert.Equal(createDto.Name, result.Name);

            _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
            _mockProductRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mockIdGenerator.Verify(gen => gen.GenerateId("PROD"), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithInvalidCategory_ReturnsNull()
        {
            // Arrange
            var createDto = new CreateProductDTO
            {
                Name = "New Product",
                CategoryId = "INVALID_CAT",
                Description = "New Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(createDto.CategoryId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _productService.CreateProductAsync(createDto);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
        }

        #endregion

        #region UpdateProductAsync Tests

        [Fact]
        public async Task UpdateProductAsync_WithValidData_UpdatesProduct()
        {
            // Arrange
            string productId = "PROD001";
            var updateDto = new UpdateProductDTO
            {
                Name = "Updated Product",
                CategoryId = "CAT001",
                Description = "Updated Description",
                ImageUrl = "updated-image.jpg"
            };

            var existingProduct = new Product
            {
                ProductId = productId,
                Name = "Original Product",
                CategoryId = "CAT001",
                Description = "Original Description",
                ImageUrl = "original-image.jpg",
                CreatedDate = DateTime.UtcNow
            };

            var category = new Category { Name = "Test Category" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(updateDto.CategoryId))
                .ReturnsAsync(category);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(productId))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var result = await _productService.UpdateProductAsync(productId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.Equal(updateDto.ImageUrl, result.ImageUrl);

            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
            _mockProductRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidProductId_ReturnsNull()
        {
            // Arrange
            string productId = "INVALID_ID";
            var updateDto = new UpdateProductDTO
            {
                Name = "Updated Product",
                CategoryId = "CAT001"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.UpdateProductAsync(productId, updateDto);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_WithInvalidCategoryId_ReturnsNull()
        {
            // Arrange
            string productId = "PROD001";
            var updateDto = new UpdateProductDTO
            {
                Name = "Updated Product",
                CategoryId = "INVALID_CAT"
            };

            var existingProduct = new Product
            {
                ProductId = productId,
                Name = "Original Product",
                CategoryId = "CAT001"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(updateDto.CategoryId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _productService.UpdateProductAsync(productId, updateDto);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        #endregion

        #region DeleteProductAsync Tests

        [Fact]
        public async Task DeleteProductAsync_WithValidId_DeletesProduct()
        {
            // Arrange
            string productId = "PROD001";
            var product = new Product
            {
                ProductId = productId,
                Name = "Test Product",
                ImageUrl = "test-image.jpg"
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            Assert.True(result);
            _mockFirebaseStorage.Verify(service => service.DeleteFileAsync(product.ImageUrl), Times.Once);
            _mockProductRepository.Verify(repo => repo.RemoveAsync(product), Times.Once);
            _mockProductRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            string productId = "INVALID_ID";
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            Assert.False(result);
            _mockProductRepository.Verify(repo => repo.RemoveAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProductAsync_WithNoImage_DoesNotCallFirebaseDelete()
        {
            // Arrange
            string productId = "PROD001";
            var product = new Product
            {
                ProductId = productId,
                Name = "Test Product",
                ImageUrl = null
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            Assert.True(result);
            _mockFirebaseStorage.Verify(service => service.DeleteFileAsync(It.IsAny<string>()), Times.Never);
            _mockProductRepository.Verify(repo => repo.RemoveAsync(product), Times.Once);
        }

        #endregion

        #region GetPagedProductsForClientAsync Tests

        [Fact]
        public async Task GetPagedProductsForClientAsync_ReturnsPagedProductViewModels()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            int totalCount = 15;

            var products = new List<Product>
            {
                new Product { ProductId = "PROD001", Name = "Product 1", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow },
                new Product { ProductId = "PROD002", Name = "Product 2", CategoryId = "CAT001", CreatedDate = DateTime.UtcNow }
            };

            var category = new Category { Name = "Test Category" };

            _mockProductRepository.Setup(repo => repo.GetPagedProductsAsync(
                pageNumber, pageSize, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true))
                .ReturnsAsync((products, totalCount));

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(category);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var (resultProducts, resultTotalCount, resultTotalPages) = await _productService.GetPagedProductsForClientAsync(
                pageNumber, pageSize);

            // Assert
            Assert.NotNull(resultProducts);
            Assert.Equal(products.Count, resultProducts.Count());
            Assert.Equal(totalCount, resultTotalCount);
            Assert.Equal((int)Math.Ceiling(totalCount / (double)pageSize), resultTotalPages);

            // Verify ViewModel properties
            var firstProduct = resultProducts.First();
            Assert.Equal(products[0].ProductId, firstProduct.ProductId);
            Assert.Equal(products[0].Name, firstProduct.Name);
            Assert.Equal(category.Name, firstProduct.CategoryName);
        }

        #endregion

        #region GetProductDetailsAsync Tests

        [Fact]
        public async Task GetProductDetailsAsync_WithValidId_ReturnsProductViewModel()
        {
            // Arrange
            string productId = "PROD001";
            var product = new Product
            {
                ProductId = productId,
                Name = "Test Product",
                CategoryId = "CAT001",
                Description = "Test Description",
                CreatedDate = DateTime.UtcNow,
                ProductRatings = new List<ProductRating>()
            };

            var category = new Category { Name = "Test Category" };

            var variants = new List<ProductVariant>
            {
                new ProductVariant {
                    VariantId = "VAR001",
                    ProductId = productId,
                    RarityId = "RAR001",
                    Price = 10.99m,
                    StockQuantity = 5
                }
            };

            var rarity = new Rarity { RarityId = "RAR001", Name = "Common" };

            var ratings = new List<ProductRating>();

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(product.CategoryId))
                .ReturnsAsync(category);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(productId))
                .ReturnsAsync(variants);

            _mockRarityRepository.Setup(repo => repo.GetByIdAsync(variants[0].RarityId))
                .ReturnsAsync(rarity);

            _mockProductRatingRepository.Setup(repo => repo.GetRatingsByProductIdAsync(productId))
                .ReturnsAsync(ratings);

            // Act
            var result = await _productService.GetProductDetailsAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(category.Name, result.CategoryName);
            Assert.NotEmpty(result.Variants);
            Assert.Equal(variants[0].Price, result.MinPrice);
            Assert.Equal(variants[0].Price, result.MaxPrice);
            Assert.Equal(rarity.Name, result.Variants.First().RarityName);
        }

        [Fact]
        public async Task GetProductDetailsAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            string productId = "INVALID_ID";

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.GetProductDetailsAsync(productId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddProductRatingAsync Tests

        [Fact]
        public async Task AddProductRatingAsync_WithValidData_AddsRating()
        {
            // Arrange
            var ratingInput = new ProductRatingInputViewModel
            {
                ProductId = "PROD001",
                Rating = 5,
                Comment = "Great product!"
            };

            string userId = "USER001";

            var product = new Product
            {
                ProductId = ratingInput.ProductId,
                Name = "Test Product"
            };

            var savedRating = new ProductRating
            {
                RatingId = "RATE001",
                ProductId = ratingInput.ProductId,
                UserId = userId,
                Rating = ratingInput.Rating,
                Comment = ratingInput.Comment,
                CreatedDate = DateTime.UtcNow,
                ApplicationUser = new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(ratingInput.ProductId))
                .ReturnsAsync(product);

            _mockProductRatingRepository.Setup(repo => repo.AddAsync(It.IsAny<ProductRating>()))
                .Returns(Task.CompletedTask);

            _mockProductRatingRepository.Setup(repo => repo.GetRatingsByProductIdAsync(ratingInput.ProductId))
                .ReturnsAsync(new List<ProductRating> { savedRating });

            // Act
            var result = await _productService.AddProductRatingAsync(ratingInput, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(savedRating.RatingId, result.RatingId);
            Assert.Equal(ratingInput.ProductId, result.ProductId);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(ratingInput.Rating, result.Rating);
            Assert.Equal(ratingInput.Comment, result.Comment);

            _mockProductRatingRepository.Verify(repo => repo.AddAsync(It.IsAny<ProductRating>()), Times.Once);
            _mockProductRatingRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddProductRatingAsync_WithInvalidProductId_ReturnsNull()
        {
            // Arrange
            var ratingInput = new ProductRatingInputViewModel
            {
                ProductId = "INVALID_PROD",
                Rating = 5,
                Comment = "Great product!"
            };

            string userId = "USER001";

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(ratingInput.ProductId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.AddProductRatingAsync(ratingInput, userId);

            // Assert
            Assert.Null(result);
            _mockProductRatingRepository.Verify(repo => repo.AddAsync(It.IsAny<ProductRating>()), Times.Never);
        }

        #endregion

        #region GetTopRatedProductsByCategoryAsync Tests

        [Fact]
        public async Task GetTopRatedProductsByCategoryAsync_WithValidCategory_ReturnsProducts()
        {
            // Arrange
            string categoryId = "CAT001";
            int limit = 5;

            var category = new Category
            {
                CategoryId = categoryId,
                Name = "Test Category"
            };

            var products = new List<Product>
            {
                new Product {
                    ProductId = "PROD001",
                    Name = "Product 1",
                    CategoryId = categoryId,
                    ProductRatings = new List<ProductRating>
                    {
                        new ProductRating { Rating = 5 },
                        new ProductRating { Rating = 4 }
                    }
                },
                new Product {
                    ProductId = "PROD002",
                    Name = "Product 2",
                    CategoryId = categoryId,
                    ProductRatings = new List<ProductRating>
                    {
                        new ProductRating { Rating = 5 }
                    }
                }
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockProductRepository.Setup(repo => repo.GetTopRatedProductsByCategoryAsync(categoryId, limit))
                .ReturnsAsync(products);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var result = await _productService.GetTopRatedProductsByCategoryAsync(categoryId, limit);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(products.Count, result.Count());

            // Verify first product's average rating calculation
            var firstProduct = result.First();
            Assert.Equal(products[0].ProductId, firstProduct.ProductId);
            Assert.Equal(4.5, firstProduct.AverageRating);
            Assert.Equal(2, firstProduct.RatingCount);
        }

        [Fact]
        public async Task GetTopRatedProductsByCategoryAsync_WithInvalidCategory_ReturnsEmptyList()
        {
            // Arrange
            string categoryId = "INVALID_CAT";
            int limit = 5;

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act
            var result = await _productService.GetTopRatedProductsByCategoryAsync(categoryId, limit);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockProductRepository.Verify(
                repo => repo.GetTopRatedProductsByCategoryAsync(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        #endregion

        #region GetRelatedProductsAsync Tests

        [Fact]
        public async Task GetRelatedProductsAsync_WithValidProductId_ReturnsRelatedProducts()
        {
            // Arrange
            string productId = "PROD001";
            int limit = 4;

            var relatedProducts = new List<Product>
            {
                new Product {
                    ProductId = "PROD002",
                    Name = "Related Product 1",
                    CategoryId = "CAT001",
                    Category = new Category { Name = "Test Category" },
                    ProductRatings = new List<ProductRating>()
                },
                new Product {
                    ProductId = "PROD003",
                    Name = "Related Product 2",
                    CategoryId = "CAT001",
                    Category = new Category { Name = "Test Category" },
                    ProductRatings = new List<ProductRating>()
                }
            };

            _mockProductRepository.Setup(repo => repo.GetRelatedProductsByCategoryAsync(productId, limit))
                .ReturnsAsync(relatedProducts);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var result = await _productService.GetRelatedProductsAsync(productId, limit);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(relatedProducts.Count, result.Count());
            Assert.Equal(relatedProducts[0].Name, result.First().Name);
        }

        [Fact]
        public async Task GetRelatedProductsAsync_WithEmptyProductId_ReturnsEmptyList()
        {
            // Arrange
            string productId = "";
            int limit = 4;

            // Act
            var result = await _productService.GetRelatedProductsAsync(productId, limit);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockProductRepository.Verify(
                repo => repo.GetRelatedProductsByCategoryAsync(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never);
        }

        #endregion

        #region GetBestSellingProductsAsync Tests

        [Fact]
        public async Task GetBestSellingProductsAsync_ReturnsBestSellingProducts()
        {
            // Arrange
            int limit = 8;

            var bestSellingProducts = new List<Product>
            {
                new Product {
                    ProductId = "PROD001",
                    Name = "Best Seller 1",
                    CategoryId = "CAT001",
                    Category = new Category { Name = "Test Category" },
                    ProductRatings = new List<ProductRating>()
                },
                new Product {
                    ProductId = "PROD002",
                    Name = "Best Seller 2",
                    CategoryId = "CAT001",
                    Category = new Category { Name = "Test Category" },
                    ProductRatings = new List<ProductRating>()
                }
            };

            _mockOrderRepository.Setup(repo => repo.GetBestSellingProductsAsync(limit))
                .ReturnsAsync(bestSellingProducts);

            _mockProductVariantRepository.Setup(repo => repo.GetVariantsByProductIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ProductVariant>());

            // Act
            var result = await _productService.GetBestSellingProductsAsync(limit);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bestSellingProducts.Count, result.Count());
            Assert.Equal(bestSellingProducts[0].Name, result.First().Name);
        }

        #endregion
    }
}
