using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace MoviesWebsite.Models.Movie
{
    [Table("Movie")]
    [Index(nameof(Title), nameof(Slug), IsUnique = true)]
    public class Movie
    {
        public int MovieId { get; set; }

        public int? GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public Group? Group { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Title")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "{0} has length from {2} to {1} characters.")]
        [Column(TypeName = "nvarchar(255)")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        [Display(Name = "Slug")]
        [Column(TypeName = "nvarchar(512)")]
        public string? Slug { get; set; }


        [Display(Name = "Trailer")]
        [MinLength(2)]
        [Column(TypeName = "nvarchar(max)")]
        public string? Trailer { get; set; }

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

        // old field in model
        [Display(Name = "Image")]
        [Column(TypeName = "nvarchar(max)")]
        public string? Image { get; set; }

        [Display(Name = "Series")]
        public bool Series { get; set; } = false;

        [Display(Name = "Duration")]
        public int? Duration { get; set; }

        public List<Category>? Categories { get; set; }

        public List<Actor>? Actors { get; set; }

        public List<Comment>? Comments { get; set; }

        public List<Evaluate>? Evaluates { get; set; }

        public List<Favourite>? Favourites { get; set; }

        public List<Episode>? Episodes { get; set; }

        public List<ImageMovie>? ImageMovies { get; set; }
    }
}
