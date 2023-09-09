using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Position { get; set; }
        public string Requirement { get; set; }
        public string Description { get; set; }
        public string? Image { get; set; }
        public bool Available { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        [ValidateNever]
        public List<Applicant>? Applicants { get; set; } = new List<Applicant>();
        public string Hours { get; set; }
        public string Salary { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public int Days { get; set; }
        public string Off { get; set; }
        public string? Location { get; set; }
    }
}
