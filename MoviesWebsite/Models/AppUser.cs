using Microsoft.AspNetCore.Identity;
using MoviesWebsite.Models.Movie;
using Org.BouncyCastle.Crypto.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Models
{
    public class AppUser : IdentityUser
    {
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Avatar")]
        [Column(TypeName = "nvarchar(max)")]
        public string? Avatar { get; set; }

        public List<Comment>? Comments { get; set; }
        public List<Evaluate>? Evaluates { get; set; }
        public List<Favourite>? Favourites { get; set; }
        public List<History>? Histories { get; set; }
    }
}
