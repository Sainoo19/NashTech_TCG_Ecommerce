using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;
using NashTech_TCG_API.Utilities;

namespace NashTech_TCG_API.Services
{
    public class RarityService : IRarityService
    {
        private readonly IRarityRepository _rarityRepository;
        private readonly IdGenerator _idGenerator;
        private readonly ILogger<RarityService> _logger;

        public RarityService(
            IRarityRepository rarityRepository,
            IdGenerator idGenerator,
            ILogger<RarityService> logger)
        {
            _rarityRepository = rarityRepository;
            _idGenerator = idGenerator;
            _logger = logger;
        }

        public async Task<RarityDTO> GetByIdAsync(string id)
        {
            var rarity = await _rarityRepository.GetByIdAsync(id);
            return rarity != null ? MapToDTO(rarity) : null;
        }

        public async Task<IEnumerable<RarityDTO>> GetAllAsync()
        {
            var rarities = await _rarityRepository.GetAllAsync();
            return rarities.Select(r => MapToDTO(r));
        }

        public async Task<(IEnumerable<RarityDTO> Rarities, int TotalCount, int TotalPages)> GetPagedRaritiesAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            var (rarities, totalCount) = await _rarityRepository.GetPagedRaritiesAsync(
                pageNumber, pageSize, searchTerm, sortBy, ascending);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var rarityDTOs = rarities.Select(r => MapToDTO(r));

            return (rarityDTOs, totalCount, totalPages);
        }

        public async Task<RarityDTO> CreateRarityAsync(CreateRarityDTO rarityDTO)
        {
            try
            {
                // Generate a prefixed ID for the rarity (RAR001, RAR002, etc.)
                string rarityId = await _idGenerator.GenerateId("RAR");

                var rarity = new Rarity
                {
                    RarityId = rarityId,
                    Name = rarityDTO.Name,
                    Description = rarityDTO.Description
                };

                await _rarityRepository.AddAsync(rarity);
                await _rarityRepository.SaveChangesAsync();

                _logger.LogInformation($"Created new rarity with ID: {rarityId}");
                return MapToDTO(rarity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rarity");
                throw;
            }
        }

        public async Task<RarityDTO> UpdateRarityAsync(string id, UpdateRarityDTO rarityDTO)
        {
            var rarity = await _rarityRepository.GetByIdAsync(id);
            if (rarity == null)
            {
                _logger.LogWarning($"Rarity not found: {id}");
                return null;
            }

            rarity.Name = rarityDTO.Name;
            rarity.Description = rarityDTO.Description;

            await _rarityRepository.UpdateAsync(rarity);
            await _rarityRepository.SaveChangesAsync();

            _logger.LogInformation($"Updated rarity: {id}");
            return MapToDTO(rarity);
        }

        public async Task<bool> DeleteRarityAsync(string id)
        {
            var rarity = await _rarityRepository.GetByIdAsync(id);
            if (rarity == null)
            {
                _logger.LogWarning($"Rarity not found: {id}");
                return false;
            }

            await _rarityRepository.RemoveAsync(rarity);
            await _rarityRepository.SaveChangesAsync();

            _logger.LogInformation($"Deleted rarity: {id}");
            return true;
        }

        private RarityDTO MapToDTO(Rarity rarity)
        {
            return new RarityDTO
            {
                RarityId = rarity.RarityId,
                Name = rarity.Name,
                Description = rarity.Description
            };
        }
    }
}
