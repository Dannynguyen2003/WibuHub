using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<StoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryConnection"))
);
builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryIdentityConnection"))
);

//builder.Services.AddDefaultIdentity<VideoUser>(options =>
builder.Services.AddIdentity<StoryUser, StoryRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // Password settings 
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    // Lockout settings 
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    // User settings 
    options.User.RequireUniqueEmail = true;
    // Sign-in settings 
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
    .AddEntityFrameworkStores<StoryIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 1. Memory Cache (bắt buộc cho Session)
builder.Services.AddDistributedMemoryCache();
// 2. Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 3. Use Session (phải trước MapControllers)
app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
