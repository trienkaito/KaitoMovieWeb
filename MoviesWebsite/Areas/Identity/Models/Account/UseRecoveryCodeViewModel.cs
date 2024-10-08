﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace MoviesWebsite.Areas.Identity.Models.AccountViewModels
{
    public class UseRecoveryCodeViewModel
    {
        
        [Required(ErrorMessage = "Must enter {0}")]
        [Display(Name = "Enter verify code have saved")]
        public string Code { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
