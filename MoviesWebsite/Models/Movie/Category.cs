using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("Category")]
    [Index(nameof(Title), nameof(Slug), IsUnique = true)]
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Title")]
        [StringLength(255,MinimumLength = 2,ErrorMessage = "{0} has length from {2} to {1} characters.")]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }


        [Display(Name = "Slug")]
        [Column(TypeName = "nvarchar(512)")]
        public string? Slug { get; set; }

        public List<Movie>? Movies { get; set; }
    }
}
