using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        [ValidateNever]
        public List<ShoppingCart>? ShoppingCarts { get; set; } = new List<ShoppingCart>();
        [Column(TypeName = "decimal(12, 2)")]
        public double TotalPrice { get; set; } = 00.00;
        [Column(TypeName = "decimal(12, 2)")]
        public double? DiscountedPrice { get; set; }
        public int TotalQuantity { get; set; } = 0;
        public bool Available { get; set; }
        public int? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        [ValidateNever]
        public Discount? Discount { get; set; }
    }
}
