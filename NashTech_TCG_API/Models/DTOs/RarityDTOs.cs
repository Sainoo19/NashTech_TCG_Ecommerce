using System.ComponentModel.DataAnnotations;

namespace NashTech_TCG_API.Models.DTOs
{
    public class RarityDTO
    {
        public string RarityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CreateRarityDTO
    {
        [Required(ErrorMessage = "Rarity name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Rarity name must be between 2 and 50 characters")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }
    }

    public class UpdateRarityDTO
    {
        [Required(ErrorMessage = "Rarity name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Rarity name must be between 2 and 50 characters")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }
    }
}
