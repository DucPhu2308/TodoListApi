using System.ComponentModel.DataAnnotations;

namespace TodoListApi.DTOs
{
    public class LoginDto
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
