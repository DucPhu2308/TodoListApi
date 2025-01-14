using System.ComponentModel.DataAnnotations;

namespace TodoListApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
