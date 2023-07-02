using System.ComponentModel.DataAnnotations;

namespace Serdiuk.Authorization.Domain.Models.Identity.DTO
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
