using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("Actor")]
    [Index(nameof(Name), nameof(Role), IsUnique = true)]
    public class Actor
    {

        [Key]
        public int ActorId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Name")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "{0} has length from {2} to {1} characters.")]
        [Column(TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        [Display(Name = "Role")]
        [Column(TypeName = "nvarchar(255)")]
        public string? Role { get; set; }

        [Display(Name = "Image")]
        [Column(TypeName = "nvarchar(255)")]
        public string? Image { get; set; }

        public List<Movie>? Movies { get; set; }
    }
}
