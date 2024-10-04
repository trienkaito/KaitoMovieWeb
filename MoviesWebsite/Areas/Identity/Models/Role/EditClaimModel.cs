using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MoviesWebsite.Areas.Identity.Models.RoleViewModels
{
  public class EditClaimModel
  {
    [Display(Name = "Enter claim (name)")]
    [Required(ErrorMessage = "Must enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} Must be from {2} to {1} characters.")]
    public string ClaimType { get; set; }

    [Display(Name = "Value")]
    [Required(ErrorMessage = "Must enter {0}")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} Must be from {2} to {1} characters.")]
    public string ClaimValue { get; set; }

    public IdentityRole role { get; set; }


  }
}
