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
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
