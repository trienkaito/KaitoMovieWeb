using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models.Movie
{
    [Table("Evaluate")]
    public class Evaluate
    {
        [Key]
        public int EvaluateId { get; set; }

        public int? MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie? Movie { get; set; }

        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }

        [Range(1, 10)]
        [Display(Name = "Star")]
        public int Star { get; set; }

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }
    }
}
