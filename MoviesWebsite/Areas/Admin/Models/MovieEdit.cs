using MoviesWebsite.Models.Movie;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MoviesWebsite.Areas.Admin.Models
{
    public class MovieEdit : Movie
    {
        [DisplayName("CategoriesIds")]
        [Required(ErrorMessage = "Please select category")]
        public int[]? CategoriesIds { get; set; }

        [DisplayName("DirectorIds")]
        [Required(ErrorMessage = "Please select director")]
        public int[]? DirectorIds { get; set; }

        [DisplayName("ActorIds")]
        [Required(ErrorMessage = "Please select actor")]
        public int[]? ActorIds { get; set; }


        [DataType(DataType.Upload)]
        [Display(Name = "Image")]
        public List<IFormFile>? FileUploads { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Trailer")]
        [Required]
        public IFormFile? TrailerUpload { get; set; }
    }
}
