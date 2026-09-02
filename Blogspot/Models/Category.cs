using System.ComponentModel.DataAnnotations;

namespace Blogspot.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = " Category Name is required")]
        [MaxLength(200, ErrorMessage = "Category Name cannot exceed 200 characters")]
        public string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}
