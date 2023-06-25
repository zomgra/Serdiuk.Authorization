﻿using System.ComponentModel.DataAnnotations;

namespace Serdiuk.Authorization.Web.Models.DTO
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
