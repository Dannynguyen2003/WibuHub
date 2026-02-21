# WibuHub - Hướng dẫn liên kết API với MVC.Customer và MVC.Admin

## 1) Chạy API trước

```bash
dotnet run --project /home/runner/work/WibuHub/WibuHub/WibuHub.API
```

Mặc định API sẽ chạy ở dạng:
- `https://localhost:7xxx`
- `http://localhost:5xxx`

Mở Swagger để kiểm tra endpoint:
- `https://localhost:7xxx/swagger`

---

## 2) Cấu hình URL API trong 2 project MVC

Thêm vào:
- `/home/runner/work/WibuHub/WibuHub/WibuHub/appsettings.json` (MVC.Admin)
- `/home/runner/work/WibuHub/WibuHub/WibuHub.MVC.Customer/appsettings.json` (MVC.Customer)

```json
"ApiSettings": {
  "BaseUrl": "https://localhost:7001/"
}
```

> Đổi `7001` theo đúng port API đang chạy trên máy của bạn.

---

## 3) Đăng ký HttpClient trong Program.cs

Trong cả 2 file Program.cs:
- `/home/runner/work/WibuHub/WibuHub/WibuHub/Program.cs`
- `/home/runner/work/WibuHub/WibuHub/WibuHub.MVC.Customer/Program.cs`

thêm:

```csharp
builder.Services.AddHttpClient("WibuHubApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
});
```

---

## 4) Gọi API từ Controller/Service (mẫu)

Ví dụ lấy danh sách truyện:

```csharp
public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("WibuHubApi");
        var stories = await client.GetFromJsonAsync<List<StoryDto>>("api/stories");
        return View(stories);
    }
}
```

---

## 5) Mapping nhanh endpoint hiện có

- `GET /api/stories`
- `GET /api/stories/{id}`
- `POST /api/stories`
- `PUT /api/stories/{id}`
- `DELETE /api/stories/{id}`
- `GET /api/categories`
- `GET /api/categories/{id}`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`

---

## 6) CORS (chỉ cần khi frontend chạy khác domain/port)

Nếu gọi API từ trình duyệt ở domain khác, bật CORS trong `WibuHub.API/Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvcClients", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7123", // MVC.Admin (xem launchSettings.json nếu đổi port)
            "https://localhost:7113"  // MVC.Customer (xem launchSettings.json nếu đổi port)
        )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("MvcClients");
```
