# README: Shared Identity Implementation
# Triển Khai Identity Chung

## English

### What is "Shared Identity"?

**Shared Identity** (Vietnamese: "xài chung 1 cái identity") means that both the Admin portal and Customer portal use the **same identity database and authentication system**. This is the current implementation in WibuHub.

### Key Characteristics

1. **One Database**: Both portals connect to `StoryIdentityDbContext`
2. **One User Table**: All users (admin and customer) are stored in `AspNetUsers`
3. **One Role System**: All roles are in `AspNetRoles`
4. **Differentiation**: Users are distinguished by:
   - `UserType` field: "Admin" or "Customer"
   - Role assignments: Admin roles vs Customer role

### Architecture Overview

```
┌──────────────────────┐
│   Admin Portal       │
│ (Port 5001)          │
│                      │
│ Registration adds:   │
│ - UserType="Admin"   │
│ - Role="Admin"       │
└──────────┬───────────┘
           │
           │  Both use same
           │  StoryIdentityDbContext
           │
           ▼
┌────────────────────────────────┐
│  Shared Identity Database      │
│  (StoryIdentityDbContext)      │
│                                │
│  ┌──────────────────────────┐ │
│  │ AspNetUsers (StoryUser)  │ │
│  │ - Admin users            │ │
│  │ - Customer users         │ │
│  └──────────────────────────┘ │
│                                │
│  ┌──────────────────────────┐ │
│  │ AspNetRoles (StoryRole)  │ │
│  │ - SuperAdmin, Admin      │ │
│  │ - ContentManager         │ │
│  │ - StoryManager           │ │
│  │ - SalesManager           │ │
│  │ - Customer               │ │
│  └──────────────────────────┘ │
└────────────────────────────────┘
           ▲
           │
           │  Both use same
           │  StoryIdentityDbContext
           │
┌──────────┴───────────┐
│  Customer Portal     │
│ (Port 5003)          │
│                      │
│ Registration adds:   │
│ - UserType="Customer"│
│ - Role="Customer"    │
└──────────────────────┘
```

### Benefits

✅ **Centralized Management**: All users in one place
✅ **Single Source of Truth**: No data duplication
✅ **Consistent Roles**: Role system works across entire application
✅ **Easy Queries**: Simple to query all users or filter by type
✅ **Scalable**: Easy to add more portals or user types

### Documentation

- **[SHARED_IDENTITY.md](SHARED_IDENTITY.md)**: Detailed architecture (Bilingual)
- **[VERIFICATION_GUIDE.md](VERIFICATION_GUIDE.md)**: Step-by-step verification (Bilingual)
- **[AUTHENTICATION.md](AUTHENTICATION.md)**: Authentication system details
- **[SETUP.md](SETUP.md)**: Setup instructions

### Quick Start

1. Apply migrations:
   ```bash
   dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
   ```

2. Start Admin portal:
   ```bash
   cd WibuHub
   dotnet run
   ```

3. Start Customer portal (in another terminal):
   ```bash
   cd WibuHub.MVC.Customer
   dotnet run
   ```

4. Verify shared database:
   ```sql
   USE StoryIdentityDbContext;
   SELECT Email, UserType FROM AspNetUsers;
   ```

---

## Tiếng Việt

### "Xài Chung Identity" Là Gì?

**Xài chung identity** có nghĩa là cả portal Admin và portal Customer đều sử dụng **cùng một database identity và hệ thống xác thực**. Đây là cách triển khai hiện tại trong WibuHub.

### Đặc Điểm Chính

1. **Một Database**: Cả hai portal kết nối đến `StoryIdentityDbContext`
2. **Một Bảng User**: Tất cả user (admin và customer) được lưu trong `AspNetUsers`
3. **Một Hệ Thống Role**: Tất cả role ở trong `AspNetRoles`
4. **Phân Biệt**: User được phân biệt bằng:
   - Trường `UserType`: "Admin" hoặc "Customer"
   - Gán role: Role admin vs Role customer

### Tổng Quan Kiến Trúc

```
┌──────────────────────┐
│   Portal Admin       │
│ (Cổng 5001)          │
│                      │
│ Đăng ký tạo:         │
│ - UserType="Admin"   │
│ - Role="Admin"       │
└──────────┬───────────┘
           │
           │  Cả hai dùng chung
           │  StoryIdentityDbContext
           │
           ▼
┌────────────────────────────────┐
│  Database Identity Chung       │
│  (StoryIdentityDbContext)      │
│                                │
│  ┌──────────────────────────┐ │
│  │ AspNetUsers (StoryUser)  │ │
│  │ - User admin             │ │
│  │ - User customer          │ │
│  └──────────────────────────┘ │
│                                │
│  ┌──────────────────────────┐ │
│  │ AspNetRoles (StoryRole)  │ │
│  │ - SuperAdmin, Admin      │ │
│  │ - ContentManager         │ │
│  │ - StoryManager           │ │
│  │ - SalesManager           │ │
│  │ - Customer               │ │
│  └──────────────────────────┘ │
└────────────────────────────────┘
           ▲
           │
           │  Cả hai dùng chung
           │  StoryIdentityDbContext
           │
┌──────────┴───────────┐
│  Portal Customer     │
│ (Cổng 5003)          │
│                      │
│ Đăng ký tạo:         │
│ - UserType="Customer"│
│ - Role="Customer"    │
└──────────────────────┘
```

### Lợi Ích

✅ **Quản Lý Tập Trung**: Tất cả user ở một chỗ
✅ **Nguồn Dữ Liệu Duy Nhất**: Không có dữ liệu trùng lặp
✅ **Role Nhất Quán**: Hệ thống role hoạt động trên toàn ứng dụng
✅ **Query Dễ Dàng**: Đơn giản để query tất cả user hoặc lọc theo loại
✅ **Mở Rộng Tốt**: Dễ thêm portal hoặc loại user mới

### Tài Liệu

- **[SHARED_IDENTITY.md](SHARED_IDENTITY.md)**: Kiến trúc chi tiết (Song ngữ)
- **[VERIFICATION_GUIDE.md](VERIFICATION_GUIDE.md)**: Hướng dẫn xác minh từng bước (Song ngữ)
- **[AUTHENTICATION.md](AUTHENTICATION.md)**: Chi tiết hệ thống xác thực
- **[SETUP.md](SETUP.md)**: Hướng dẫn cài đặt

### Bắt Đầu Nhanh

1. Áp dụng migration:
   ```bash
   dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
   ```

2. Chạy portal Admin:
   ```bash
   cd WibuHub
   dotnet run
   ```

3. Chạy portal Customer (terminal khác):
   ```bash
   cd WibuHub.MVC.Customer
   dotnet run
   ```

4. Xác minh database chung:
   ```sql
   USE StoryIdentityDbContext;
   SELECT Email, UserType FROM AspNetUsers;
   ```

---

## Evidence / Bằng Chứng

### Configuration Files / File Cấu Hình

**Admin Portal** (`WibuHub/appsettings.json`):
```json
"StoryIdentityConnection": "Server=., 1433; Database=StoryIdentityDbContext; ..."
```

**Customer Portal** (`WibuHub.MVC.Customer/appsettings.json`):
```json
"StoryIdentityConnection": "Server=., 1433; Database=StoryIdentityDbContext; ..."
```

### Program.cs Configuration

**Both files use**:
```csharp
builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryIdentityConnection"))
);

builder.Services.AddIdentity<StoryUser, StoryRole>(options => { ... })
    .AddEntityFrameworkStores<StoryIdentityDbContext>()
    .AddDefaultTokenProviders();
```

### Database Schema

```sql
-- Check users from both portals in SAME table
SELECT Email, UserType, CreatedAt 
FROM AspNetUsers 
ORDER BY CreatedAt DESC;

-- Result shows users from BOTH portals:
-- superadmin@wibuhub.com    | Admin    | ...
-- admin.test@wibuhub.com     | Admin    | ...
-- customer.test@wibuhub.com  | Customer | ...
```

## Conclusion / Kết Luận

### English
WibuHub successfully implements a **true shared identity system** where both Admin and Customer portals use the same authentication database. This provides centralized user management, consistent security, and efficient data storage.

### Tiếng Việt
WibuHub triển khai thành công **hệ thống identity chung thực sự** với cả portal Admin và Customer đều dùng chung database xác thực. Điều này cung cấp quản lý user tập trung, bảo mật nhất quán và lưu trữ dữ liệu hiệu quả.

---

## Contact / Liên Hệ

For questions or issues, please refer to:
- Project Issues on GitHub
- Documentation in this repository

Có câu hỏi hoặc vấn đề, vui lòng tham khảo:
- Issues của dự án trên GitHub
- Tài liệu trong repository này
