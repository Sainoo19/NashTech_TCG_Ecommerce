using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NashTech_TCG_API.Models
{
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CartItemId { get; set; }

        [ForeignKey("Cart")]
        public string CartId { get; set; }

        [ForeignKey("ProductVariant")]
        public string VariantId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // Navigation properties
        [Required]
        public virtual Cart Cart { get; set; } = null!;
        [Required]
        public virtual ProductVariant ProductVariant { get; set; } = null!;
    }
}
