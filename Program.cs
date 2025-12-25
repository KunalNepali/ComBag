using ComBag.Data;
using ComBag.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ========== 1. CONFIGURE SERVICES ==========

// Add MVC services with Newtonsoft.Json support
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        // Configure Newtonsoft.Json serialization settings
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== 2. CONFIGURE ASP.NET CORE IDENTITY ==========
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configure Identity options for MVP
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole>(); 


// ========== 3. ENABLE SESSION SERVICES WITH NEWTONSOFT.JSON ==========
// For distributed memory cache (session storage)
builder.Services.AddDistributedMemoryCache();

// Configure session with Newtonsoft.Json serialization
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".ComBag.Session";
});

// For advanced session serialization, you might want to use Newtonsoft.Json
// This is optional but recommended for complex objects
//builder.Services.AddSingleton<Microsoft.AspNetCore.Http.ISessionStore, Microsoft.AspNetCore.Session.DistributedSessionStore>();

// Add Razor Pages support (required for Identity UI)
builder.Services.AddRazorPages();

var app = builder.Build();

// ========== 4. CONFIGURE MIDDLEWARE PIPELINE ==========

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ========== IMPORTANT: Authentication BEFORE Authorization ==========
app.UseAuthentication();
app.UseAuthorization();

// ========== 5. ENABLE SESSION MIDDLEWARE ==========
app.UseSession();

// Map controllers and Razor pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
// Seed database with admin user and sample data
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();