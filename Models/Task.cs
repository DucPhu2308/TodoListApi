using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApi.Models
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }
        public string Title { get; set; } = null!;

        [DefaultValue(false)]
        public bool IsCompleted { get; set; } = false;

        [BindNever]
        public int UserId { get; set; }
        [BindNever]
        public User? User { get; set; }
    }
}
