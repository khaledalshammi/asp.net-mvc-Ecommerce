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
    
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTimeOffset Start { get; set; }
        [Required]
        public DateTimeOffset End { get; set; }
        public bool Available { get; set; }
        public int? TypeId { get; set; }
        [ForeignKey(nameof(TypeId))]
        public SubscriptionType? Type { get; set; }
        public string? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
