using NashTech_TCG_ShareViewModels.ViewModels;

namespace NashTech_TCG_MVC.Services.Interfaces
{
    public interface IHomeService
    {
        Task<(bool Success, string Message, IEnumerable<CategoryViewModel> Data)> GetAllCategoriesAsync();
        Task<(bool Success, string Message, IEnumerable<ProductViewModel> Data)> GetTopRatedProductsByCategoryAsync(string categoryId, int limit = 8);
    }
}
