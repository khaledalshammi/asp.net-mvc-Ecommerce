using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string PostalCode { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        [Required]
        [ValidateNever]
        public List<Cart> Carts { get; set; }
        [ValidateNever]
        public List<Comment>? Comments { get; set; } = new List<Comment>();
        [ValidateNever]
        public List<Inquiry>? Inquiries { get; set; }
        [ValidateNever]
        public bool EnabledNotification { get; set; } = false;
        public List<Notification>? Notifications { get; set; } = new List<Notification>();
        [ValidateNever]
        public List<Order>? Orders { get; set; }
        [ValidateNever]
        public List<ShoppingCart>? ShoppingCarts { get; set;}
        [ValidateNever]
        public List<News>? News { get; set;}
        [ValidateNever]
        public List<Job>? Jobs { get; set;}
        public int? SubscriptionId { get; set; }
        [ForeignKey("SubscriptionId")]
        [ValidateNever]
        public Subscription? Subscription { get; set; }
        [ValidateNever]
        public List<Role> Role { get; set; } = new List<Role>();
        public List<Review>? Reviews { get; set; } = new List<Review>();
        public List<Applicant>? Applicants { get; set; } = new List<Applicant>();
    }
}

