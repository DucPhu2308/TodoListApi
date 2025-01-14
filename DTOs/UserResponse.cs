using TodoListApi.Models;

namespace TodoListApi.DTOs
{
    public class UserResponse
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
