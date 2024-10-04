// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebsite.Areas.Identity.Models.ManageViewModels
{
    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = "Must enter {0}")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long..", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Cofirmation password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
