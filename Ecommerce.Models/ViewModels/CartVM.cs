using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Models.ViewModels
{
    public class CartVM
    {
        public Cart Cart { get; set; }
        public List<ShoppingCart> ShoppingCarts { get; set; }
        public Discount DiscountCode { get; set; } 
        public string ValidCode { get; set; }
        public User User { get; set; }
    }
}
