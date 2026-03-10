using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using WibuHub.API.Models;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.ApplicationCore.Entities.Identity;
using WibuHub.DataLayer;
using WibuHub.Service.EmailSender;
using WibuHub.Service.Implementations;
using WibuHub.Service.Implementations.EmailSender;
using WibuHub.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Đăng ký DbContext (Kết nối CSDL)
// Bạn cần chắc chắn chuỗi kết nối trong appsettings.json tên là "DefaultConnection" hoặc sửa lại cho khớp
builder.Services.AddDbContext<StoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryConnection")));

builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryIdentityConnection")));

// 2. Đăng ký Service (Dependency Injection)
// AddScoped: Service được tạo mới cho mỗi HTTP Request (phù hợp với DbContext)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
//builder.Services.AddScoped<IChapterService, ChapterService>();
// 3. Configure MoMo Settings
builder.Services.Configure<MomoSettings>(builder.Configuration.GetSection("MomoSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, MimeKitEmailSender>();
// 4. Register HttpClient for MoMo API calls with timeout
builder.Services.AddHttpClient<MomoPaymentService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
// 5. Register Payment Service
builder.Services.AddScoped<IPaymentService, MomoPaymentService>();
// 6. Add Controllers
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
// 7. Add Authentication and Authorization
builder.Services.AddIdentity<StoryUser, StoryRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<StoryIdentityDbContext>()
    .AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>();
if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException("Jwt settings are not configured.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();