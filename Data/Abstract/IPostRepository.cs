using BlogApp.Entity;

namespace BlogApp.Data.Abstract
{
    public interface IPostRepository
    {
        IQueryable<Post> Posts { get; }
        void CreatePost(Post post);
        void EditPost(Post post);
        void EditPost(Post post, int[] tagIds);
        void RemovePost(Post post);
        Task AddNewPostAsync(string title, string description, string content, string url, string image, int userId);
        Task<bool> IsUrlInUseAsync(string url, int? excludePostId = null);
        Task<List<Post>> GetPostsByUserAsync(int userId);
        Task<int> GetUserPostCountAsync(int userId);
        Task<string> GetPostTagsAsync(int postId);
    }
}
