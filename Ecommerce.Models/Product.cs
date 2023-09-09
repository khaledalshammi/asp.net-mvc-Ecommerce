using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        [MinLength(4)]
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        [ValidateNever]
        public List<Image> Images { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        [ValidateNever]
        public List<Comment>? Comments { get; set; } = new List<Comment>();
        public int Likes { get; set; } = 0;
        public int DisLikes { get; set; } = 0;
        public List<int>? RelatedProductsId { get; set; } = new List<int>();
        public List<Review>? Reviews { get; set; } = new List<Review>();
        public double? AVGReviews { get; set; } = 0;
    }
}
