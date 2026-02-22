using Microsoft.EntityFrameworkCore;
using WibuHub.DataLayer;
using WibuHub.Service.Implementations;
using WibuHub.Service.Interface;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();
const string adminApplicationPartName = "WibuHub.MVC.Admin";
// Remove Admin application part to prevent ambiguous route matching when Customer app starts.
var adminPart = mvcBuilder.PartManager.ApplicationParts.FirstOrDefault(part => part.Name == adminApplicationPartName);
if (adminPart is not null)
{
    mvcBuilder.PartManager.ApplicationParts.Remove(adminPart);
}
// Add DbContext
builder.Services.AddDbContext<StoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryConnection"))
);
builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryConnection"))
);

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<StoryIdentityDbContext>();
// Register services
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must run before authorization so user identity is available for policy checks.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
