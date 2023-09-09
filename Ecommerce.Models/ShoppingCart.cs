using Abp;
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
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }
        public User User { get; set; }
        public Cart Cart { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public double TotalPrice { get; set; } = 00.00;
    }
}
