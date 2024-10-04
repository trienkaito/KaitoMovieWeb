﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesWebsite.Areas.Identity.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Must enter {0}")]
        [Display(Name = "Email address or username", Prompt = "Email address or username")]
        public string UserNameOrEmail { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Prompt="Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
