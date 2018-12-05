using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        public string Gender { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        public DateTime CreatedOn { get; set; }        
        public DateTime LastActive { get; set; }
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "Password shouldn't be shorted than 6 and longer that 32 characters")]
        public string Password { get; set; }

        public UserForRegisterDto()
        {
            CreatedOn = DateTime.UtcNow;
            LastActive = DateTime.UtcNow;
        }
    }
}