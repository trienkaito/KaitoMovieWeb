using System.ComponentModel.DataAnnotations;

namespace MoviesWebsite.Areas.Identity.Models.RoleViewModels
{
  public class CreateRoleModel
    {
        [Display(Name = "Enter role name")]
        [Required(ErrorMessage = "Must enter {0}")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} Must be from {2} to {1} characters.")]
        public string Name { get; set; }


    }
}
