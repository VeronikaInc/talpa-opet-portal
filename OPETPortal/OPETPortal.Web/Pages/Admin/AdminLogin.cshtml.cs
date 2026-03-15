using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace OPETPortal.Web.Pages.Admin;

public class AdminLoginModel : PageModel
{
    private readonly IConfiguration _config;

    public AdminLoginModel(IConfiguration config)
    {
        _config = config;
    }

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect("/admin");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string username, string password)
    {
        var expectedUser = _config["Admin:Username"] ?? "admin";
        var expectedPass = _config["Admin:Password"] ?? "changeme";

        if (username == expectedUser && password == expectedPass)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Redirect("/admin");
        }

        ErrorMessage = "Kullanıcı adı veya şifre hatalı.";
        return Page();
    }
}
