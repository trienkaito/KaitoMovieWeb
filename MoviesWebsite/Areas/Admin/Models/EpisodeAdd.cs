using MoviesWebsite.Models.Movie;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MoviesWebsite.Areas.Admin.Models
{
    public class EpisodeAdd 
    {
        [Key]
        public int EpisodeId { get; set; }

        public int? MovieId { get; set; }

        [Required]
        [Display(Name = "Title")]
        [StringLength(255, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Display(Name = "Slug")]
        [Column(TypeName = "nvarchar(512)")]
        public string? Slug { get; set; }

        [DataType(DataType.Upload)]
        [Required]
        [Display(Name = "Link")]
        public IFormFile Link { get; set; }
    }
}
