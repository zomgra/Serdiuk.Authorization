using System.ComponentModel.DataAnnotations;

namespace Serdiuk.Authorization.Web.Models.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
