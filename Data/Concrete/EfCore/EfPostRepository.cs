using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data.Concrete.EfCore
{
    public class EfPostRepository : IPostRepository
    {
        private BlogContext _context;

        public EfPostRepository(BlogContext context)
        {
            _context = context;
        }

        public IQueryable<Post> Posts => _context.Posts;

        public void CreatePost(Post post)
        {
            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        public async Task AddNewPostAsync(
            string title,
            string description,
            string content,
            string url,
            string image,
            int userId
        )
        {
            await _context.Database.ExecuteSqlRawAsync(
                "CALL AddNewPost({0}, {1}, {2}, {3}, {4}, {5})",
                title,
                description,
                content,
                url,
                image,
                userId
            );
        }

        public void EditPost(Post post)
        {
            var entity = _context.Posts.FirstOrDefault(p => p.PostId == post.PostId);

            if (entity != null)
            {
                entity.Title = post.Title;
                entity.Description = post.Description;
                entity.Content = post.Content;
                entity.Url = post.Url;
                entity.Image = post.Image;
                entity.IsActive = post.IsActive;

                _context.Posts.Update(entity);
                _context.SaveChanges();
            }
        }

        public void EditPost(Post post, int[] tagIds)
        {
            var entity = _context
                .Posts.Include(p => p.Tags)
                .FirstOrDefault(p => p.PostId == post.PostId);
            var tags = _context.Tags.Where(t => tagIds.Contains(t.TagId)).ToList();

            if (entity != null)
            {
                entity.Title = post.Title;
                entity.Description = post.Description;
                entity.Content = post.Content;
                entity.Url = post.Url;
                entity.IsActive = post.IsActive;
                entity.Tags = tags;

                if (post.Image != null)
                    entity.Image = post.Image;

                _context.SaveChanges();
            }
        }

        public void RemovePost(Post post)
        {
            var entity = _context
                .Posts.Include(p => p.Comments)
                .FirstOrDefault(p => p.PostId == post.PostId);

            if (entity != null)
            {
                var comments = entity.Comments;

                foreach (var comment in comments)
                {
                    _context.Comments.Remove(comment);
                }

                _context.Posts.Remove(entity);

                _context.SaveChanges();
            }
        }

        public async Task<bool> IsUrlInUseAsync(string url, int? excludePostId = null)
        {
            var query = _context.Posts.Where(p => p.Url == url);

            if (excludePostId.HasValue)
            {
                query = query.Where(p => p.PostId != excludePostId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<Post>> GetPostsByUserAsync(int userId)
        {
            var posts = await _context
                .Posts.FromSqlRaw("CALL GetPostsOfUser({0})", userId)
                .Include(p => p.User)
                .ToListAsync();
            return posts;
        }

        public async Task<int> GetUserPostCountAsync(int userId)
        {
            var result = await _context
                .Database.SqlQuery<int>($"SELECT GetPostCountByUser({userId})")
                .FirstOrDefaultAsync();
            return result;
        }

        public async Task<string> GetPostTagsAsync(int postId)
        {
            var result = await _context
                .Database.SqlQuery<string>($"SELECT GetTagsByPost({postId})")
                .FirstOrDefaultAsync();
            return result ?? string.Empty;
        }
    }
}
