using System.Security.Claims;
using BlogApp.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlogApp.Services;

public class CookieService : ICookieService
{
    public async Task SetUserCookies(HttpContext httpContext, User user)
    {
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.GivenName, user.Name ?? ""),
            new Claim(ClaimTypes.UserData, user.Image ?? "")
        };

        if (user.Email == "admin@deneme.com")
        {
            userClaims.Add(new Claim(ClaimTypes.Role, "admin"));
        }

        var claimsIdentity = new ClaimsIdentity(
            userClaims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );
    }
}