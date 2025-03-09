using System.Security.Claims;
using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    public class PostsController : Controller
    {
        private IPostRepository _postRepository;
        private ICommentRepository _commentRepository;
        private ITagRepository _tagRepository;

        public PostsController(IPostRepository postRepository, ICommentRepository commentRepository, ITagRepository tagRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index(string tag)
        {
            var posts = _postRepository.Posts.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(tag))
            {
                posts = posts.Where(p => p.Tags.Any(t => t.Url == tag));
            }
            return View(new PostsViewModel { Posts = await posts.ToListAsync() });
        }

        public async Task<IActionResult> Details(string url)
        {
            return View(
                await _postRepository
                    .Posts
                    .Include(p => p.User)
                    .Include(p => p.Tags)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(p => p.Url == url)
            );
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddComment(int PostId, string Text)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);
            var fullname = User.FindFirstValue(ClaimTypes.GivenName);
            var avatar = User.FindFirstValue(ClaimTypes.UserData);

            var entity = new Comment
            {
                PostId = PostId,
                Text = Text,
                PublishedOn = DateTime.Now,
                UserId = int.Parse(userId ?? ""),
            };

            _commentRepository.CreateComment(entity);

            return Json(
                new
                {
                    username,
                    fullname,
                    Text,
                    entity.PublishedOn,
                    avatar,
                }
            );
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _postRepository.IsUrlInUseAsync(model.Url))
                {
                    ModelState.AddModelError("Url", "Bu URL zaten kullanımda. Lütfen başka bir URL seçin.");
                    return View(model);
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                string imageFileName = "1.jpg";

                if (model.ImageUpload != null)
                {
                    var extension = Path.GetExtension(model.ImageFileName!).ToLowerInvariant();
                    var randomFileName = string.Format($"{Guid.NewGuid()}{extension}");
                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/img",
                        randomFileName
                    );

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(stream);
                    }

                    imageFileName = randomFileName;
                }

                await _postRepository.AddNewPostAsync(model.Title, model.Description, model.Content, model.Url,
                    imageFileName, userId);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
            var role = User.FindFirstValue(ClaimTypes.Role);

            var posts = _postRepository.Posts;

            if (string.IsNullOrEmpty(role))
            {
                posts = posts.Where(p => p.UserId == userId);
            }

            return View(await posts.ToListAsync());
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = _postRepository.Posts.Include(p => p.Tags).FirstOrDefault(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            ViewBag.Tags = _tagRepository.Tags.ToList();

            return View(new PostCreateViewModel
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Content = post.Content,
                Url = post.Url,
                IsActive = post.IsActive,
                Tags = post.Tags
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(PostCreateViewModel model, int[] tagIds)
        {
            if (ModelState.IsValid)
            {
                if (await _postRepository.IsUrlInUseAsync(model.Url, model.PostId))
                {
                    ModelState.AddModelError("Url", "Bu URL zaten kullanımda. Lütfen başka bir URL seçin.");
                    ViewBag.Tags = _tagRepository.Tags.ToList();
                    return View(model);
                }

                var entityToUpdate = new Post
                {
                    PostId = model.PostId,
                    Title = model.Title,
                    Description = model.Description,
                    Content = model.Content,
                    Url = model.Url,
                    IsActive = model.IsActive
                };

                if (model.ImageUpload != null)
                {
                    var extension = Path.GetExtension(model.ImageFileName!).ToLowerInvariant();
                    var randomFileName = string.Format($"{Guid.NewGuid()}{extension}");
                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/img",
                        randomFileName
                    );

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(stream);
                    }

                    entityToUpdate.Image = randomFileName;
                }

                if (User.FindFirstValue(ClaimTypes.Role) == "admin")
                {
                    entityToUpdate.IsActive = model.IsActive;
                }

                _postRepository.EditPost(entityToUpdate, tagIds);
                return RedirectToAction("List");
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = _postRepository.Posts.Include(p => p.Tags).FirstOrDefault(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userRole != "admin" && post.UserId.ToString() != userId)
            {
                return Forbid();
            }

            _postRepository.RemovePost(post);

            return RedirectToAction("List");
        }
    }
}
