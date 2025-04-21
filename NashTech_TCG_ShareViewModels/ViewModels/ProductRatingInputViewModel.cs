using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NashTech_TCG_ShareViewModels.ViewModels
{
    public class ProductRatingInputViewModel
    {
        [Required]
        public string ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }
    }
}
