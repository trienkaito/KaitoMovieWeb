using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("Favourite")]
    public class Favourite
    {
        [Key]
        public int FavouriteId { get; set; }

        public int? MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie? Movie { get; set; }

        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }
    }
}
