# Shared Identity System - Hệ Thống Identity Chung

## Overview / Tổng Quan

WibuHub sử dụng **một hệ thống Identity chung** (shared identity system) cho cả Admin portal và Customer portal.

**English**: WibuHub uses a **single shared identity system** for both Admin and Customer portals.

## Architecture / Kiến Trúc

```
┌─────────────────────┐
│   Admin Portal      │
│ (WibuHub.MVC.Admin) │
└──────────┬──────────┘
           │
           │  Uses / Sử dụng
           │  StoryIdentityDbContext
           │
           ▼
┌──────────────────────────────────┐
│  Shared Identity Database        │
│  (StoryIdentityDbContext)        │
│                                  │
│  Tables / Bảng:                  │
│  - AspNetUsers (StoryUser)       │
│  - AspNetRoles (StoryRole)       │
│  - AspNetUserRoles               │
│  - Other Identity tables         │
└──────────────────────────────────┘
           ▲
           │
           │  Uses / Sử dụng
           │  StoryIdentityDbContext
           │
┌──────────┴──────────┐
│  Customer Portal    │
│(WibuHub.MVC.Customer)│
└─────────────────────┘
```

## Key Features / Tính Năng Chính

### 1. Same Database / Cùng Database
Both portals connect to the **same Identity database**: `StoryIdentityDbContext`

**Vietnamese**: Cả hai portal kết nối đến **cùng một database Identity**: `StoryIdentityDbContext`

**Connection String** (in both `appsettings.json`):
```json
"StoryIdentityConnection": "Server=., 1433; Database=StoryIdentityDbContext; User Id=sa; password=Danh@2003; TrustServerCertificate=True; Trusted_Connection=False; MultipleActiveResultSets=true;"
```

### 2. Same User Entity / Cùng Thực Thể User
Both portals use: `StoryUser` (from `WibuHub.ApplicationCore.Entities.Identity`)

**Fields / Trường:**
- `Id`: User identifier
- `Email`: User email (unique)
- `UserName`: Username
- `FullName`: Full name
- `Avatar`: Avatar image
- **`UserType`**: "Admin" hoặc "Customer" / "Admin" or "Customer"
- `CreatedAt`: Account creation date
- All standard Identity fields

### 3. Same Role Entity / Cùng Thực Thể Role
Both portals use: `StoryRole` (from `WibuHub.ApplicationCore.Entities.Identity`)

**Roles / Các Role:**
- SuperAdmin
- Admin
- ContentManager
- StoryManager
- SalesManager
- Customer

### 4. Shared Configuration / Cấu Hình Chung

**Admin Portal** (`WibuHub/Program.cs`):
```csharp
builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryIdentityConnection"))
);

builder.Services.AddIdentity<StoryUser, StoryRole>(options => { ... })
    .AddEntityFrameworkStores<StoryIdentityDbContext>()
    .AddDefaultTokenProviders();
```

**Customer Portal** (`WibuHub.MVC.Customer/Program.cs`):
```csharp
builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryIdentityConnection"))
);

builder.Services.AddIdentity<StoryUser, StoryRole>(options => { ... })
    .AddEntityFrameworkStores<StoryIdentityDbContext>()
    .AddDefaultTokenProviders();
```

## Benefits / Lợi Ích

### 1. Centralized User Management / Quản Lý User Tập Trung
- All users stored in one place / Tất cả user lưu ở một nơi
- Easy to manage and query / Dễ quản lý và truy vấn
- Single source of truth / Nguồn dữ liệu duy nhất

### 2. Role-Based Differentiation / Phân Biệt Theo Role
- Admin users have admin roles / User admin có role admin
- Customer users have customer role / User customer có role customer
- UserType field provides additional distinction / Trường UserType cung cấp phân biệt bổ sung

### 3. Potential Cross-Portal Access / Khả Năng Truy Cập Cross-Portal
- A user account exists in both contexts / Một tài khoản user tồn tại trong cả hai context
- Access controlled by roles and UserType / Truy cập được kiểm soát bởi role và UserType
- Could enable admin users to access customer features if needed / Có thể cho phép admin user truy cập tính năng customer nếu cần

## How It Works / Cách Hoạt Động

### Registration / Đăng Ký

**Admin Registration** (Admin portal):
```
1. User registers at /Identity/Account/Register
2. System creates StoryUser with:
   - UserType = "Admin"
   - Assigned role = "Admin"
3. User saved to shared database
```

**Customer Registration** (Customer portal):
```
1. User registers at /Identity/Account/Register
2. System creates StoryUser with:
   - UserType = "Customer"
   - Assigned role = "Customer"
3. User saved to shared database
```

### Authentication / Xác Thực

Both portals authenticate against the same user table:
- Check email/password against AspNetUsers
- Load user roles from AspNetUserRoles
- Create authentication cookie for the portal

**Vietnamese**: Cả hai portal xác thực với cùng bảng user:
- Kiểm tra email/password với AspNetUsers
- Load role user từ AspNetUserRoles
- Tạo authentication cookie cho portal

### Authorization / Phân Quyền

Access is controlled by:
1. **Authentication**: User must be logged in
2. **Roles**: User must have appropriate role
3. **Controllers**: Use `[Authorize(Roles = "...")]` attribute

**Example**:
```csharp
[Authorize(Roles = "SuperAdmin,Admin,StoryManager")]
public class StoriesController : Controller { }
```

## Database Schema / Cấu Trúc Database

### AspNetUsers (StoryUser)
```sql
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) PRIMARY KEY,
    Email NVARCHAR(256) UNIQUE,
    UserName NVARCHAR(256),
    FullName NVARCHAR(200),
    Avatar NVARCHAR(500),
    UserType NVARCHAR(50) NOT NULL DEFAULT 'Customer',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    -- Standard Identity fields
    PasswordHash NVARCHAR(MAX),
    SecurityStamp NVARCHAR(MAX),
    EmailConfirmed BIT,
    ...
);
```

### AspNetRoles (StoryRole)
```sql
CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(256),
    Description NVARCHAR(500),
    -- Standard Identity fields
    ...
);
```

### AspNetUserRoles (Many-to-Many)
```sql
CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450),
    RoleId NVARCHAR(450),
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
);
```

## Querying Users / Truy Vấn Users

### Get All Admin Users
```csharp
var adminUsers = await _context.Users
    .Where(u => u.UserType == "Admin")
    .ToListAsync();
```

### Get All Customers
```csharp
var customers = await _context.Users
    .Where(u => u.UserType == "Customer")
    .ToListAsync();
```

### Get Users by Role
```csharp
var contentManagers = await _userManager.GetUsersInRoleAsync("ContentManager");
```

## Security Considerations / Cân Nhắc Bảo Mật

### 1. Separate Authentication Cookies
Currently, each portal has its own authentication cookie:
- Admin cookie is for Admin portal
- Customer cookie is for Customer portal

**Vietnamese**: Hiện tại mỗi portal có cookie xác thực riêng:
- Cookie Admin cho portal Admin
- Cookie Customer cho portal Customer

### 2. Role-Based Access Control
Access to features is controlled by roles, not just by which portal the user is using.

**Vietnamese**: Truy cập tính năng được kiểm soát bởi role, không chỉ bởi portal nào user đang dùng.

### 3. UserType Field
Provides additional layer of differentiation between admin and customer accounts.

**Vietnamese**: Cung cấp lớp phân biệt bổ sung giữa tài khoản admin và customer.

## Migration / Di Chuyển

To apply the shared identity schema:
```bash
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
```

This creates the shared identity database used by both portals.

**Vietnamese**: Để áp dụng schema identity chung:
```bash
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
```

Điều này tạo database identity chung được sử dụng bởi cả hai portal.

## Testing / Kiểm Tra

### Verify Shared Database / Xác Minh Database Chung

1. Register a user in Admin portal
2. Check database - user appears in AspNetUsers
3. Register a user in Customer portal
4. Check database - both users are in same AspNetUsers table
5. Verify UserType field differentiates them

**Vietnamese**: 
1. Đăng ký user trong portal Admin
2. Kiểm tra database - user xuất hiện trong AspNetUsers
3. Đăng ký user trong portal Customer
4. Kiểm tra database - cả hai user đều ở cùng bảng AspNetUsers
5. Xác minh trường UserType phân biệt họ

### SQL Query to Verify
```sql
-- See all users with their types
SELECT Id, Email, UserName, UserType, CreatedAt
FROM AspNetUsers
ORDER BY CreatedAt DESC;

-- Count users by type
SELECT UserType, COUNT(*) as Count
FROM AspNetUsers
GROUP BY UserType;
```

## Future Enhancements / Cải Tiến Tương Lai

### 1. Shared Authentication Cookie
Enable single sign-on across both portals:
```csharp
// Configure same cookie name and settings in both portals
options.Cookie.Name = "WibuHub.Auth";
options.Cookie.Domain = ".wibuhub.com";
```

### 2. Unified Login Page
Create a single login page that redirects based on user type.

### 3. Admin Access to Customer Features
Allow admin users to access customer portal with their admin credentials.

## Conclusion / Kết Luận

WibuHub implements a **true shared identity system** where:
- ✅ One database stores all users
- ✅ One user entity (StoryUser)
- ✅ One role system (StoryRole)
- ✅ Both portals use same identity infrastructure
- ✅ Differentiation by UserType and Roles

**Vietnamese**: WibuHub triển khai **hệ thống identity chung thực sự** với:
- ✅ Một database lưu tất cả user
- ✅ Một entity user (StoryUser)
- ✅ Một hệ thống role (StoryRole)
- ✅ Cả hai portal dùng cùng cơ sở hạ tầng identity
- ✅ Phân biệt bằng UserType và Role

This architecture provides flexibility, centralization, and clean separation of concerns while maintaining a single source of truth for all user accounts.

**Vietnamese**: Kiến trúc này cung cấp tính linh hoạt, tập trung hóa và tách biệt rõ ràng các mối quan tâm trong khi duy trì nguồn dữ liệu duy nhất cho tất cả tài khoản người dùng.
