using Microsoft.EntityFrameworkCore;

namespace TodoListApi.Models
{
    public class TodoListContext : DbContext
    {
        public TodoListContext(DbContextOptions<TodoListContext> options)
                    : base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Task>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            // Seed data
            modelBuilder.Entity<Role>()
                .HasData(
                    new Role { Id = 1, Name = "Manager" },
                    new Role { Id = 2, Name = "Employee" }
                 );
            modelBuilder.Entity<User>()
                .HasData(new User { Id = 3, Username = "user3", Password = BCrypt.Net.BCrypt.HashPassword("1234"), Email = "abc@a.com", RoleId = 1 });
        }
    }
}
