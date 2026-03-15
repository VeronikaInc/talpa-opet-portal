using Microsoft.AspNetCore.Authentication.Cookies;
using OPETPortal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Override config from environment variable
var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING");
if (!string.IsNullOrEmpty(connStr))
    builder.Configuration["ConnectionStrings:Default"] = connStr;

var adminUser = Environment.GetEnvironmentVariable("Admin__Username");
if (!string.IsNullOrEmpty(adminUser))
    builder.Configuration["Admin:Username"] = adminUser;

var adminPass = Environment.GetEnvironmentVariable("Admin__Password");
if (!string.IsNullOrEmpty(adminPass))
    builder.Configuration["Admin:Password"] = adminPass;

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.Cookie.Name = "opet_admin";
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUyeService, UyeService>();
builder.Services.AddScoped<ISistemAyarlariService, SistemAyarlariService>();
builder.Services.AddSingleton<LocalizationService>();
builder.Services.AddSingleton<DbInitService>();

var app = builder.Build();

// Initialize DB
using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<DbInitService>();
    var retries = 5;
    while (retries-- > 0)
    {
        try { await dbInit.InitializeAsync(); break; }
        catch { if (retries == 0) throw; await Task.Delay(2000); }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
