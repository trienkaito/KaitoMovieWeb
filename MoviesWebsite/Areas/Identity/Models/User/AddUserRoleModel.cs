using System.Collections.Generic;
using System.ComponentModel;
using MoviesWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MoviesWebsite.Areas.Identity.Models.UserViewModels
{
  public class AddUserRoleModel
  {
    public AppUser user { get; set; }

    [DisplayName("All role of user")]
    public string[] RoleNames { get; set; }

    public List<IdentityRoleClaim<string>> claimsInRole { get; set; }
    public List<IdentityUserClaim<string>> claimsInUserClaim { get; set; }

  }
}