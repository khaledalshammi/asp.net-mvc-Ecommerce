using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Applicant
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? CV { get; set; }
        public User? User { get; set; }
        public int? JobId { get; set; }
        [ForeignKey("JobId")]
        public Job? Job { get; set; }
        public DateTimeOffset AppliedAt { get; set; }
    }
}
