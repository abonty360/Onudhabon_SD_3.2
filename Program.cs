using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Onudhabon.Data;
using Onudhabon.Models;
using Onudhabon.Services;

// Load environment variables from .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add Database Context
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"];

var postgresConnectionString = PostgresConnectionHelper.ConvertUrlToConnectionString(rawConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

// Cloudinary & Storage Services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// AI Chat Service (Gemini Cloud API or Local Ollama)
var aiProvider = builder.Configuration["AiProvider"] ?? "Gemini";
if (string.Equals(aiProvider, "Ollama", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<ILlmChatService, OllamaChatService>();
}
else
{
    builder.Services.AddSingleton<ILlmChatService, GeminiChatService>();
}

// Password Hasher for User
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Cookie Authentication with Session Expiration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Onudhabon.AuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session timeout duration
        options.SlidingExpiration = true;                 // Resets expiration window on user activity
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Session State Services (for HttpContext.Session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "Onudhabon.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed initial database data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        DbInitializer.SeedAdminUser(context, hasher);
        DbInitializer.SeedClassPlans(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();