using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MoviesWebsite.Models.Movie
{
    public class ActorViewModel
    {
        public Actor Actor { get; set; }
        [ValidateNever]
        public List<SelectListItem> Roles { get; set; }
        public string? imgUrl { get; set; }
    }
}
