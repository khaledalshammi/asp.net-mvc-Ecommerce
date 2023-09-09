using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Models.ViewModels
{
    public class ProductReviewVM
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
        public Review? Review { get; set; }
        public User? User { get; set; }
        public Like? Like { get; set; }
        public DisLike? DisLike { get; set; }
        public List<Comment>? Comments { get; set; }
        public bool Exists { get; set; }
    }
}
