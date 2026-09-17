using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Onudhabon.Data;
using Onudhabon.Models;
using Onudhabon.Services;

// Enable legacy timestamp behavior for Npgsql to handle DateTime conversions smoothly
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Load environment variables from .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add Database Context (PostgreSQL via DATABASE_URL from .env or configuration)
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("DATABASE_URL not found in .env or configuration.");

var npgsqlConnectionString = PostgresConnectionHelper.ConvertUrlToConnectionString(rawConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(npgsqlConnectionString));

// Cloudinary & Storage Services
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IVolunteerRankingService, VolunteerRankingService>();

// Payment Gateway Services (Sandbox & Production)
builder.Services.AddScoped<ISSLCommerzService, SSLCommerzService>();

// HttpClient and Memory Cache
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// AI & Knowledge Services (Gemini Cloud API & PDF Knowledge Base)
builder.Services.AddSingleton<ILlmChatService, GeminiChatService>();
builder.Services.AddScoped<IPdfKnowledgeService, PdfKnowledgeService>();

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

// Add CORS for Gateway Callbacks
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseCors();

// Private Network Access (PNA) header support for gateway callbacks from public HTTPS to localhost
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
    {
        context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Prevent browser from caching auth pages or authenticated pages (prevents back-button access after login)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
    if (path.StartsWith("/account/login") || 
        path.StartsWith("/account/register") || 
        path.StartsWith("/admin") || 
        context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "-1";
    }
    await next();
});

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
        context.Database.Migrate();
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        DbInitializer.SeedAdminUser(context, hasher);
        DbInitializer.SeedClassPlans(context);
        DbInitializer.SeedStudents(context);
        DbInitializer.SeedForumPosts(context);
        DbInitializer.SeedDonations(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
