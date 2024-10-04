// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebsite.Areas.Identity.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Must enter {0}")]
        [EmailAddress(ErrorMessage = "Wrong email format")]
        [Display(Name = "Email", Prompt = "Email")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Must enter {0}")]
        [StringLength(100, ErrorMessage = "{0} Must be from {2} to {1} characters.", MinimumLength = 2)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmation password", Prompt = "Confirmation password")]
        [Compare("Password", ErrorMessage = "Confirmation password is not correct.")]
        public string ConfirmPassword { get; set; }


        [DataType(DataType.Text)]
        [Display(Name = "User name", Prompt = "User name")]
        [Required(ErrorMessage = "Must enter {0}")]
        [StringLength(100, ErrorMessage = "{0} Must be from {2} to {1} characters.", MinimumLength = 3)]
        public string UserName { get; set; }

    }
}
