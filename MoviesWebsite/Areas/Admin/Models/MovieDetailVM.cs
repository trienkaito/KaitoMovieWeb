using MoviesWebsite.Models.Movie;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MoviesWebsite.Areas.Admin.Models
{
    public class MovieDetailVM
    {
        public int MovieId { get; set; }

        public int? GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public Group? Group { get; set; }

        [Required]
        [Display(Name = "Title")]
        [Length(255, 2)]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        [Display(Name = "Slug")]
        [MinLength(2)]
        [Column(TypeName = "nvarchar(512)")]
        public string? Slug { get; set; }

        [Required]
        [Display(Name = "Trailer")]
        [MinLength(2)]
        [Column(TypeName = "nvarchar(max)")]
        public string Trailer { get; set; }

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }

        [Display(Name = "Release Date")]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; } = false;

        [Column(TypeName = "float(2)")]
        public float? Rate { get; set; } = 0f;

        public int? View { get; set; } = 0;

        [Display(Name = "Image")]
        [Column(TypeName = "nvarchar(max)")]
        public string? Image { get; set; }

        [Display(Name = "Series")]
        public bool Series { get; set; } = false;

        public List<Category>? Categories { get; set; }

        public List<Actor>? Actors { get; set; }

        public List<Comment>? Comments { get; set; }

        public List<Evaluate>? Evaluates { get; set; }

        public List<Episode>? Episodes { get; set; }
    }
}
