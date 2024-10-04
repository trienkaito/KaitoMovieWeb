using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("Episode")]
    [Index(nameof(Title), nameof(Slug), IsUnique = true)]
    public class Episode
    {
        [Key]
        public int EpisodeId { get; set; }

        public int? MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie? Movie { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Title")]
        [StringLength(255,MinimumLength = 2,ErrorMessage = "{0} has length from {2} to {1} characters.")]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Display(Name = "Slug")]
        [Column(TypeName = "nvarchar(512)")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Link")]
        [MinLength(2)]
        [Column(TypeName = "nvarchar(max)")]
        public string Link { get; set; }

		public List<History>? Histories { get; set; }
	}
}
