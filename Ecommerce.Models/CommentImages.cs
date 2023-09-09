using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class CommentImages
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
        [ValidateNever]
        public int CommentId { get; set; }
        [ForeignKey("CommentId")]
        [ValidateNever]
        public Comment Comment { get; set; }
    }
}
