using BlogApp.Entity;

namespace BlogApp.Services;

public interface ICookieService
{
    Task SetUserCookies(HttpContext httpContext, User user);
}