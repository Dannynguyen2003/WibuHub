# Verification Guide - Shared Identity System
# Hướng Dẫn Xác Minh - Hệ Thống Identity Chung

## Quick Verification / Xác Minh Nhanh

This guide helps you verify that both Admin and Customer portals are using the same shared identity database.

### Prerequisites / Yêu Cầu
- SQL Server running / SQL Server đang chạy
- Database migrations applied / Đã áp dụng migration
- Both portals accessible / Cả hai portal có thể truy cập

## Step 1: Check Configuration / Kiểm Tra Cấu Hình

### Admin Portal Configuration
File: `WibuHub/appsettings.json`
```json
"StoryIdentityConnection": "Server=., 1433; Database=StoryIdentityDbContext; ..."
```

### Customer Portal Configuration
File: `WibuHub.MVC.Customer/appsettings.json`
```json
"StoryIdentityConnection": "Server=., 1433; Database=StoryIdentityDbContext; ..."
```

**✅ Verify**: Both use the **same database name**: `StoryIdentityDbContext`

## Step 2: Check Program.cs Configuration

### Admin Portal
File: `WibuHub/Program.cs`
```csharp
builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryIdentityConnection"))
);
```

### Customer Portal
File: `WibuHub.MVC.Customer/Program.cs`
```csharp
builder.Services.AddDbContext<StoryIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoryIdentityConnection"))
);
```

**✅ Verify**: Both use `StoryIdentityDbContext` with `StoryIdentityConnection`

## Step 3: Apply Migrations / Áp Dụng Migration

```bash
cd /home/runner/work/WibuHub/WibuHub

# Apply identity migrations
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
```

**Expected Output / Kết Quả Mong Đợi:**
```
Build succeeded.
Applying migration 'xxx_Identity_xxx'.
Applying migration 'xxx_AddUserTypeAndCreatedAt'.
Done.
```

## Step 4: Verify Database Schema / Xác Minh Schema Database

Connect to SQL Server and run:

```sql
-- Check if database exists
SELECT name FROM sys.databases WHERE name = 'StoryIdentityDbContext';

-- Check tables
USE StoryIdentityDbContext;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**Expected Tables / Bảng Mong Đợi:**
- AspNetRoles
- AspNetRoleClaims
- AspNetUsers ← **Most important!**
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens

## Step 5: Register Test Users / Đăng Ký User Thử Nghiệm

### Register Admin User

1. Start Admin portal:
   ```bash
   cd WibuHub
   dotnet run
   ```

2. Navigate to: `https://localhost:5001/Identity/Account/Register`

3. Register a user:
   - Email: `admin.test@wibuhub.com`
   - Full Name: `Admin Test User`
   - Password: `AdminTest@123`

4. Check database:
   ```sql
   SELECT Id, Email, UserName, UserType, CreatedAt 
   FROM AspNetUsers 
   WHERE Email = 'admin.test@wibuhub.com';
   ```

**Expected / Mong Đợi:**
- UserType = `'Admin'`
- User exists in database

### Register Customer User

1. Start Customer portal:
   ```bash
   cd WibuHub.MVC.Customer
   dotnet run
   ```

2. Navigate to: `https://localhost:5003/Identity/Account/Register`

3. Register a user:
   - Email: `customer.test@wibuhub.com`
   - Full Name: `Customer Test User`
   - Password: `CustomerTest@123`

4. Check database:
   ```sql
   SELECT Id, Email, UserName, UserType, CreatedAt 
   FROM AspNetUsers 
   WHERE Email = 'customer.test@wibuhub.com';
   ```

**Expected / Mong Đợi:**
- UserType = `'Customer'`
- User exists in database

## Step 6: Verify Shared Database / Xác Minh Database Chung

Run this query to see both users in the **same table**:

```sql
SELECT 
    Email,
    UserName,
    UserType,
    CreatedAt,
    EmailConfirmed
FROM AspNetUsers
ORDER BY CreatedAt DESC;
```

**Expected Results / Kết Quả Mong Đợi:**
```
Email                           | UserType  | CreatedAt
--------------------------------|-----------|---------------------------
customer.test@wibuhub.com       | Customer  | 2026-01-30 10:00:00
admin.test@wibuhub.com          | Admin     | 2026-01-30 09:55:00
superadmin@wibuhub.com          | Admin     | 2026-01-30 09:50:00
```

**✅ Verification Successful**: All users are in the **same table** with different UserType values!

## Step 7: Verify Roles / Xác Minh Roles

```sql
-- Check all roles
SELECT Id, Name, Description 
FROM AspNetRoles
ORDER BY Name;
```

**Expected Roles / Roles Mong Đợi:**
- Admin
- ContentManager
- Customer
- SalesManager
- StoryManager
- SuperAdmin

```sql
-- Check user roles assignments
SELECT 
    u.Email,
    r.Name as RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
ORDER BY u.Email;
```

**Expected / Mong Đợi:**
```
Email                       | RoleName
----------------------------|----------
admin.test@wibuhub.com      | Admin
customer.test@wibuhub.com   | Customer
superadmin@wibuhub.com      | SuperAdmin
```

## Step 8: Count Users by Type / Đếm User Theo Loại

```sql
SELECT 
    UserType,
    COUNT(*) as TotalUsers
FROM AspNetUsers
GROUP BY UserType;
```

**Expected / Mong Đợi:**
```
UserType  | TotalUsers
----------|------------
Admin     | 2
Customer  | 1
```

## Step 9: Verify Connection Strings Match / Xác Minh Connection String Khớp

```bash
# Check Admin portal
grep -A 2 "StoryIdentityConnection" WibuHub/appsettings.json

# Check Customer portal
grep -A 2 "StoryIdentityConnection" WibuHub.MVC.Customer/appsettings.json
```

**✅ Verify**: Both should show the **exact same connection string**

## Step 10: Test Login / Kiểm Tra Đăng Nhập

### Admin Login
1. Go to: `https://localhost:5001/Identity/Account/Login`
2. Login with: `admin.test@wibuhub.com` / `AdminTest@123`
3. **Expected**: Successfully logged in to Admin portal

### Customer Login
1. Go to: `https://localhost:5003/Identity/Account/Login`
2. Login with: `customer.test@wibuhub.com` / `CustomerTest@123`
3. **Expected**: Successfully logged in to Customer portal

## Verification Checklist / Danh Sách Kiểm Tra

- [ ] Both `appsettings.json` use same connection string
- [ ] Both `Program.cs` use `StoryIdentityDbContext`
- [ ] Database `StoryIdentityDbContext` exists
- [ ] Table `AspNetUsers` contains users from both portals
- [ ] Admin users have `UserType = 'Admin'`
- [ ] Customer users have `UserType = 'Customer'`
- [ ] All 6 roles exist in `AspNetRoles`
- [ ] Users are correctly assigned to roles
- [ ] Can login to Admin portal with admin user
- [ ] Can login to Customer portal with customer user

## Troubleshooting / Xử Lý Sự Cố

### Issue: Users not in same database
**Solution**: Check connection strings - they must be identical

### Issue: UserType is NULL
**Solution**: Run migration to add UserType column:
```bash
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
```

### Issue: Roles not created
**Solution**: Ensure `RoleInitializer` is called in `Program.cs`:
```csharp
using (var scope = app.Services.CreateScope())
{
    await RoleInitializer.InitializeAsync(services);
}
```

### Issue: Cannot see both users
**Solution**: Make sure both portals are using correct connection string name:
```csharp
builder.Configuration.GetConnectionString("StoryIdentityConnection")
```

## Success Indicators / Chỉ Số Thành Công

✅ **Shared Identity System is Working** if:
1. Both portals connect to database `StoryIdentityDbContext`
2. Users from both portals appear in same `AspNetUsers` table
3. UserType field correctly differentiates Admin vs Customer
4. All roles exist in single `AspNetRoles` table
5. Users can authenticate in their respective portals

## Conclusion / Kết Luận

If all steps pass, you have successfully verified that WibuHub uses a **true shared identity system** where both Admin and Customer portals share the same authentication database and infrastructure.

**Vietnamese**: Nếu tất cả các bước đều pass, bạn đã xác minh thành công rằng WibuHub sử dụng **hệ thống identity chung thực sự** với cả Admin và Customer portal chia sẻ cùng database xác thực và cơ sở hạ tầng.

---

For more information, see:
- [SHARED_IDENTITY.md](SHARED_IDENTITY.md) - Architecture details
- [AUTHENTICATION.md](AUTHENTICATION.md) - Authentication system overview
- [SETUP.md](SETUP.md) - Setup instructions
