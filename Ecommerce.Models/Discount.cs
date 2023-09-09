using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Discount
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ConcurrencyCheck]
        public string Code { get; set; }
        [ValidateNever]
        public int? CartId { get; set; }
        [ValidateNever]
        [ForeignKey("CartId")]
        public Cart? Cart { get; set; }
        [Required]
        public int Percentage { get; set; }
        [Required]
        public bool Available { get; set; } = true;
    }
}
