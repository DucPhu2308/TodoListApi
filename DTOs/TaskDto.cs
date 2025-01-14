using System.ComponentModel;

namespace TodoListApi.DTOs
{
    public class TaskDto
    {
        public string? Title { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
