﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MoviesWebsite.Areas.Identity.Models.AccountViewModels
{
    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        public string Code { get; set; }

        public string? ReturnUrl { get; set; }

        [Display(Name = "Remmeber this browser?")]
        public bool RememberBrowser { get; set; }

        [Display(Name = "Remeber me?")]
        public bool RememberMe { get; set; }
    }
}
