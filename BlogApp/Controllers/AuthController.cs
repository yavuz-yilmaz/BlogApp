using System.Security.Claims;
using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers;

public class AuthController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly ICookieService _cookieService;

    public AuthController(IUserRepository userRepository, ICookieService cookieService)
    {
        _userRepository = userRepository;
        _cookieService = cookieService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Posts");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var isUser = _userRepository.Users.FirstOrDefault(u =>
                u.Email == model.Email && u.Password == model.Password
            );

            if (isUser != null)
            {
                await _cookieService.SetUserCookies(HttpContext, isUser);

                TempData["WelcomeMessage"] = "Hoş geldiniz, " + isUser.Name + "!";
                return RedirectToAction("Index", "Posts");
            }
            else
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış.");
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Posts");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var isEmailInUse = await _userRepository.Users.AnyAsync(u => u.Email == model.Email);

            if (isEmailInUse)
            {
                ModelState.AddModelError(
                    "Email",
                    "Girdiğiniz e posta başka bir kullanıcı tarafından kullanılıyor."
                );
                return View(model);
            }

            var isUserNameInUse = await _userRepository.Users.AnyAsync(u =>
                u.UserName == model.UserName
            );

            if (isUserNameInUse)
            {
                ModelState.AddModelError(
                    "UserName",
                    "Girdiğiniz kullanıcı adı başka bir kullanıcı tarafından kullanılıyor."
                );
                return View(model);
            }

            var user = _userRepository.CreateUser(
                new User
                {
                    UserName = model.UserName,
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    Image = "avatar.jpg",
                }
            );

            await _cookieService.SetUserCookies(HttpContext, user);
            TempData["WelcomeMessage"] = "Hoş geldiniz, " + user.Name + "!";
            return RedirectToAction("Index", "Posts");
        }

        return View(model);
    }
}
