using System.ComponentModel.DataAnnotations;

namespace BlogApp.Entity
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; } // NOT NULL
        public string Name { get; set; } // NOT NULL
        public string Email { get; set; } // NOT NULL, Unique
        public string Password { get; set; } // NOT NULL
        public string Image { get; set; }

        public List<Post> Posts { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
