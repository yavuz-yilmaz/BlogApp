using System.Security.Claims;
using BlogApp.Data.Abstract;
using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ICookieService _cookieService;

        public UsersController(IUserRepository userRepository, ICookieService cookieService)
        {
            _userRepository = userRepository;
            _cookieService = cookieService;
        }
        
        [HttpGet]
        public IActionResult Profile(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            var user = _userRepository
                .Users.Include(u => u.Posts)
                .Include(u => u.Comments)
                .ThenInclude(c => c.Post)
                .FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
            var user = _userRepository.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(
                new EditProfileViewModel
                {
                    UserName = user.UserName,
                    Name = user.Name,
                    Email = user.Email,
                    Image = user.Image,
                    Message = TempData["Message"] as string,
                    AlertColor = TempData["AlertColor"] as string,
                }
            );
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");

            var isUserNameInUse = await _userRepository.Users.AnyAsync(u =>
                u.UserName == model.UserName && u.UserId != userId
            );
            if (isUserNameInUse)
            {
                ModelState.AddModelError(
                    "UserName",
                    "Kullanıcı adı başka bir kullanıcı tarafından kullanılıyor."
                );
            }

            var IsEmailInUse = await _userRepository.Users.AnyAsync(u =>
                u.Email == model.Email && u.UserId != userId
            );
            if (IsEmailInUse)
            {
                ModelState.AddModelError(
                    "Email",
                    "Eposta adresi başka bir kullanıcı tarafından kullanılıyor."
                );
            }

            if (ModelState.IsValid)
            {
                var user = await _userRepository.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return NotFound();
                }

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

                    user.Image = randomFileName;
                }

                user.UserName = model.UserName;
                user.Name = model.Name;
                user.Email = model.Email;

                var updatedUser = await _userRepository.EditUser(user);
                if (updatedUser != null)
                {
                    await _cookieService.SetUserCookies(HttpContext, updatedUser);

                    TempData["Message"] = "Hesabınız başarıyla güncellendi!";
                    TempData["AlertColor"] = "success";
                    return RedirectToAction("EditProfile");
                }
                else
                {
                    return View(
                        new EditProfileViewModel
                        {
                            Message = "Hesabınız güncellenirken bir hata ile karşılaşıldı.",
                            AlertColor = "danger",
                            UserName = user.UserName,
                            Name = user.Name,
                            Email = user.Email,
                            Image = user.Image,
                        }
                    );
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
            var user = await _userRepository.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (
                !string.IsNullOrEmpty(model.CurrentPassword)
                && (model.CurrentPassword != user.Password)
            )
            {
                ModelState.AddModelError("CurrentPassword", "Hatalı şifre");
            }

            if (ModelState.IsValid)
            {
                _userRepository.ChangeUserPassword(user, model.NewPassword!);

                ViewBag.Message =
                    "Şifreniz başarıyla değiştirildi!   Ana sayfaya yönlendiriliyorsunuz...";
                ViewBag.AlertColor = "success";

                return View();
            }

            return View(model);
        }
    }
}
