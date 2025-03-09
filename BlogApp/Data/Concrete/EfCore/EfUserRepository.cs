using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data.Concrete.EfCore
{
    public class EfUserRepository : IUserRepository
    {
        private BlogContext _context;

        public EfUserRepository(BlogContext context)
        {
            _context = context;
        }

        public IQueryable<User> Users => _context.Users;

        public User CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public async Task<User?> EditUser(User user)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (entity != null)
            {
                entity.UserName = user.UserName;
                entity.Name = user.Name;
                entity.Email = user.Email;
                entity.Image = user.Image;

                _context.Users.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }

            return null;
        }

        public async void ChangeUserPassword(User user, string newPassword)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (entity != null)
            {
                entity.Password = newPassword;

                _context.Users.Update(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
