using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("Group")]
    [Index(nameof(Name),IsUnique = true)]
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Name")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "{0} has length from {2} to {1} characters.")]
        [Column(TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        public List<Movie>? Movies { get; set; }
    }
}
