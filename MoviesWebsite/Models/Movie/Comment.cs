using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime;

namespace MoviesWebsite.Models.Movie
{
    [Table("Comment")]
    [Index(nameof(Content))]
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public int? MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie? Movie { get; set; }

        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }

        public int? CommentParentId { get; set; }

        [ForeignKey(nameof(CommentParentId))]
        public Comment? CommentParent { get; set; }

        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Update Date")]
        public DateTime? UpdateDate { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [Display(Name = "Content")]
        [Column(TypeName = "nvarchar(2048)")]
        public string Content { get; set; }

        [Display(Name = "Hiden")]
        public bool Hiden { get; set; } = false;

        public List<Comment>? CommentChildren { get; set; }
    }
}
