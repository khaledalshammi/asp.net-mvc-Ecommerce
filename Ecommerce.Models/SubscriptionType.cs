using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public class SubscriptionType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public string Description { get; set; }
        public List<Subscription>? Subscriptions { get; set; } = new List<Subscription>();
        [Required]
        [DisplayName("Discount Percentage")]
        public int DiscountAmount { get; set; }
        [Required]
        [DisplayName("Cart Discount Percentage")]
        public int CartDiscountPercentage { get; set; }
        [Required]
        [DisplayName("Discount Codes for user")]
        public int NumberOfDiscount { get; set; }
        [Required]
        public string Image { get; set; }
    }
}
