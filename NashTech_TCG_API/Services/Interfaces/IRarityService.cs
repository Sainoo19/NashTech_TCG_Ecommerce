using NashTech_TCG_API.Models.DTOs;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface IRarityService
    {
        Task<RarityDTO> GetByIdAsync(string id);
        Task<IEnumerable<RarityDTO>> GetAllAsync();
        Task<(IEnumerable<RarityDTO> Rarities, int TotalCount, int TotalPages)> GetPagedRaritiesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
        Task<RarityDTO> CreateRarityAsync(CreateRarityDTO rarityDTO);
        Task<RarityDTO> UpdateRarityAsync(string id, UpdateRarityDTO rarityDTO);
        Task<bool> DeleteRarityAsync(string id);
    }
}
