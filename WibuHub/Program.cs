using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.Common.Contants;
using WibuHub.DataLayer;
using WibuHub.Service.EmailSender;

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

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

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

// Enable routing for areas (e.g., /Admin/Stories)


app.MapControllerRoute(
    name: "Customer",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<StoryRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<StoryUser>>();

    await RoleSeeder.SeedRolesAsync(roleManager);

    var superAdmin = await userManager.FindByEmailAsync(AppConstants.SuperAdminEmail);
    if (superAdmin != null && !await userManager.IsInRoleAsync(superAdmin, AppConstants.Roles.SuperAdmin))
    {
        await userManager.AddToRoleAsync(superAdmin, AppConstants.Roles.SuperAdmin);
    }
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<StoryRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<StoryUser>>();

    // 1. Vẫn gọi RoleSeeder của ông để nạp danh sách Role
    await RoleSeeder.SeedRolesAsync(roleManager);

    string superAdminEmail = AppConstants.SuperAdminEmail;
    string superAdminRole = AppConstants.Roles.SuperAdmin;

    // 2. [TÍNH ĐỘC TÔN] Quét và tước quyền tất cả những kẻ mạo danh
    var allSuperAdmins = await userManager.GetUsersInRoleAsync(superAdminRole);
    foreach (var admin in allSuperAdmins)
    {
        if (!string.Equals(admin.Email, superAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            await userManager.RemoveFromRoleAsync(admin, superAdminRole);
        }
    }

    // 3. Tự động TẠO LẠI nếu tài khoản bị ông lỡ tay xóa
    var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
    if (superAdmin == null)
    {
        superAdmin = new StoryUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            EmailConfirmed = true // Cho pass luôn để đăng nhập được ngay
        };

        // Nhớ thay "Admin@123!" bằng mật khẩu mặc định ông muốn set nhé
        await userManager.CreateAsync(superAdmin, "Admin@123!");
    }

    // 4. Đảm bảo tài khoản này phải cầm quyền SuperAdmin
    if (!await userManager.IsInRoleAsync(superAdmin, superAdminRole))
    {
        await userManager.AddToRoleAsync(superAdmin, superAdminRole);
    }
}

app.Run();
