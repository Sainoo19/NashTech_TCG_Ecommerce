using NashTech_TCG_API.Models.DTOs;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO> GetByIdAsync(string id);
        Task<IEnumerable<CategoryDTO>> GetAllAsync();
        Task<(IEnumerable<CategoryDTO> Categories, int TotalCount, int TotalPages)> GetPagedCategoriesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
        Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO categoryDTO);
        Task<CategoryDTO> UpdateCategoryAsync(string id, UpdateCategoryDTO categoryDTO);
        Task<bool> DeleteCategoryAsync(string id);
    }
}
