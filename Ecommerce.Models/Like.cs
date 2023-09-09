using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
