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
    public class Inquiry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Type { get; set; }
        public string Content { get; set; }
        public string? ImgUrl { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public User User { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool? Closed { get; set; } = false;
        public bool? Reply { get; set; } = false;
        public DateTimeOffset? ClosedAt { get; set; }
    }
}
