using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("ImageMovie")]
    public class ImageMovie
    {
        [Key]
        public int ImageId { get; set; }
        
        public string? Url { get; set; }

        public int? MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie? Movie { get; set; }
    }
}
