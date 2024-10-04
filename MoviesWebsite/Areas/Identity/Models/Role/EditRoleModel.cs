    using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MoviesWebsite.Areas.Identity.Models.RoleViewModels
{
  public class EditRoleModel
    {
        [Display(Name = "Role name")]
        [Required(ErrorMessage = "Must enter {0}")]
        [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} Must be from {2} to {1} characters.")]
        public string Name { get; set; }
        public List<IdentityRoleClaim<string>>? Claims { get; set; }

        public IdentityRole? role { get; set; }




    }
}
