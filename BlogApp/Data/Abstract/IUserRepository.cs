using BlogApp.Entity;

namespace BlogApp.Data.Abstract
{
    public interface IUserRepository
    {
        IQueryable<User> Users { get; }
        User CreateUser(User user);
        Task<User?> EditUser(User user);
        void ChangeUserPassword(User user, string newPassword);
    }
}
