using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using University.Data;
using University.DataExtension;
using University.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<UnivercityDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")!));

// Identity + Roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<UnivercityDbContext>()
.AddDefaultTokenProviders();

// Cookies: auto-logout after 10 minutes of inactivity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // DEV-friendly. In production you can set Always.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

app.ApplyMigrations();
using (var scope = app.Services.CreateScope())
{
    await SeedData.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// Prevent cached authenticated pages (fixes "Back button shows logged-in page" illusion)
app.Use(async (context, next) =>
{
    await next();

    var isHtml = context.Response.ContentType?.StartsWith("text/html") == true;
    if (!isHtml) return;

    if (context.User?.Identity?.IsAuthenticated == true ||
        context.Request.Path.StartsWithSegments("/Calendar") ||
        context.Request.Path.StartsWithSegments("/Admin") ||
        context.Request.Path.StartsWithSegments("/Students"))
    {
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
        context.Response.Headers["Vary"] = "Cookie";
    }
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();