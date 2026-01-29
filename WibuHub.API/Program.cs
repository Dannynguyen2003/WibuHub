using Microsoft.EntityFrameworkCore;
using WibuHub.ApplicationCore.Configuration;
using WibuHub.DataLayer;
using WibuHub.Service.Implementations;
using WibuHub.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Đăng ký DbContext (Kết nối CSDL)
// Bạn cần chắc chắn chuỗi kết nối trong appsettings.json tên là "DefaultConnection" hoặc sửa lại cho khớp
builder.Services.AddDbContext<StoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryConnection")));

// 2. Đăng ký Service (Dependency Injection)
// AddScoped: Service được tạo mới cho mỗi HTTP Request (phù hợp với DbContext)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPaymentService, MomoPaymentService>();

// 3. Configure MoMo Settings
builder.Services.Configure<MomoSettings>(builder.Configuration.GetSection("MomoSettings"));

// 4. Register HttpClient for MoMo API calls
builder.Services.AddHttpClient<IPaymentService, MomoPaymentService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();