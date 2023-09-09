using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [ConcurrencyCheck]
        public string Title { get; set; }
        [DisplayName("Status")]
        public bool? Valid { get; set; }
        public List<User>? Users { get; set; }
    }
}
