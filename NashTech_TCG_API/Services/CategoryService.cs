using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Utilities;

namespace NashTech_TCG_API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IdGenerator _idGenerator;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IdGenerator idGenerator,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _idGenerator = idGenerator;
            _logger = logger;
        }

        public async Task<CategoryDTO> GetByIdAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null ? MapToDTO(category) : null;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => MapToDTO(c));
        }

        public async Task<(IEnumerable<CategoryDTO> Categories, int TotalCount, int TotalPages)> GetPagedCategoriesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            var (categories, totalCount) = await _categoryRepository.GetPagedCategoriesAsync(
                pageNumber, pageSize, searchTerm, sortBy, ascending);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var categoryDTOs = categories.Select(c => MapToDTO(c));

            return (categoryDTOs, totalCount, totalPages);
        }

        public async Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO categoryDTO)
        {
            try
            {
                // Generate a prefixed ID for the category (CAT001, CAT002, etc.)
                string categoryId = await _idGenerator.GenerateId("CAT");

                var category = new Category
                {
                    CategoryId = categoryId,
                    Name = categoryDTO.Name,
                    Description = categoryDTO.Description
                };

                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();

                _logger.LogInformation($"Created new category with ID: {categoryId}");
                return MapToDTO(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(string id, UpdateCategoryDTO categoryDTO)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning($"Category not found: {id}");
                return null;
            }

            category.Name = categoryDTO.Name;
            category.Description = categoryDTO.Description;

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation($"Updated category: {id}");
            return MapToDTO(category);
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning($"Category not found: {id}");
                return false;
            }

            await _categoryRepository.RemoveAsync(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation($"Deleted category: {id}");
            return true;
        }

        private CategoryDTO MapToDTO(Category category)
        {
            return new CategoryDTO
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
