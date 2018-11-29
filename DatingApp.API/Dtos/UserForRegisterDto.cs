using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "Password shouldn't be shorted than 6 and longer that 32 characters")]
        public string Password { get; set; }
    }
}