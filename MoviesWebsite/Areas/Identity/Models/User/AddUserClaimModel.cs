using System.ComponentModel.DataAnnotations;

namespace MoviesWebsite.Areas.Identity.Models.UserViewModels
{
  public class AddUserClaimModel
  {
    [Display(Name = "Claim (name) type")]
    [Required(ErrorMessage = "Must enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} Must be from {2} to {1} characters.")]
    public string ClaimType { get; set; }

    [Display(Name = "Value")]
    [Required(ErrorMessage = "Must enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} Must be from {2} to {1} characters.")]
    public string ClaimValue { get; set; }

  }
}