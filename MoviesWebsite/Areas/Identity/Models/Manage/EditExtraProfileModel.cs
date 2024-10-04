using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesWebsite.Areas.Identity.Models.ManageViewModels
{
  public class EditExtraProfileModel
  {
      [Display(Name = "Avatar")]
      public string? Avatar { get; set; }
      
      [DataType(DataType.Upload)]
      [Display(Name = "Avatar")]
      public IFormFile? Image { get; set; }

      [Display(Name = "User name")]
      public string? UserName { get; set; }

      [Display(Name = "Email address")]
      public string? UserEmail { get; set; }
      [Display(Name = "Phone number")]
      public string? PhoneNumber { get; set; }

      [Display(Name = "Birth date")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

  }
}