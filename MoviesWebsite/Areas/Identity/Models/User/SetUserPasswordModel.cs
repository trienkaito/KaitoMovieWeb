using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MoviesWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MoviesWebsite.Areas.Identity.Models.UserViewModels
{
  public class SetUserPasswordModel
  {
      [Required(ErrorMessage = "Must enter {0}")]
      [StringLength(100, ErrorMessage = "{0} Must be from {2} to {1} characters.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "New password")]
      public string NewPassword { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirmation password")]
      [Compare("NewPassword", ErrorMessage = "The password and confirm password do not match.")]
      public string ConfirmPassword { get; set; }


  }
}