using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    public class News
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? Url { get; set; }
        public string? UrlDescription { get; set; }
    }
}
