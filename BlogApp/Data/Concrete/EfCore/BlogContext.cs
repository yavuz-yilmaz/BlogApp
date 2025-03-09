using BlogApp.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data.Concrete.EfCore
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options)
            : base(options) { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            

            // Unique constraint on Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Check constraint on Tag.Color
            modelBuilder.Entity<Tag>()
                .HasCheckConstraint("CHK_Tags_Color", "Color BETWEEN 0 AND 10");

            // Default value for Post.IsActive
            modelBuilder.Entity<Post>()
                .Property(p => p.IsActive)
                .HasDefaultValue(true);
        }
    }   
}
