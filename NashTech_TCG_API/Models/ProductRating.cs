using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NashTech_TCG_API.Models
{
    public class ProductRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string RatingId { get; set; }

        [ForeignKey("Product")]
        public string ProductId { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [Required]
        public virtual Product Product { get; set; } = null!;
        [Required]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
