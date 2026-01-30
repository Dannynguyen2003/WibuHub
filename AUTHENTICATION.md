# WibuHub Authentication System

This document describes the authentication and role-based access control system implemented for WibuHub.

## Overview

The system uses a **shared identity system** (hệ thống identity chung) where both Admin and Customer portals use the same database and user management infrastructure. This provides centralized user management while maintaining separate access controls.

The system provides separate authentication pages for two types of users:
- **Admin Users**: System administrators with various roles for managing content
- **Customer Users**: End users who read and purchase stories

> **Important**: Both portals use the **same Identity database** (`StoryIdentityDbContext`), ensuring all users are stored in one central location. See [SHARED_IDENTITY.md](SHARED_IDENTITY.md) for detailed architecture.

## User Roles

### Admin Roles
1. **SuperAdmin**: Full system access with all privileges
2. **Admin**: General administrative access
3. **ContentManager**: Manages content - uploads and translates stories
4. **StoryManager**: Manages stories and chapters
5. **SalesManager**: Manages sales and orders

### Customer Role
- **Customer**: Regular users who read and purchase stories

## Default Credentials

A default SuperAdmin account is automatically created on first run:
- **Email**: superadmin@wibuhub.com
- **Password**: SuperAdmin@123

## Architecture

### Shared Identity System

Both Admin and Customer portals share the same identity infrastructure:

```
Admin Portal ──┐
               ├──▶ StoryIdentityDbContext (Shared Database)
Customer Portal┘
```

**Key Points:**
- Same database: `StoryIdentityDbContext`
- Same user table: `AspNetUsers` (StoryUser entity)
- Same roles table: `AspNetRoles` (StoryRole entity)
- Differentiation by `UserType` field ("Admin" or "Customer")
- Access control through ASP.NET Core Identity roles

### Projects Structure
- **WibuHub.MVC.Admin**: Admin portal with authentication for administrators
- **WibuHub.MVC.Customer**: Customer portal with authentication for customers
- **WibuHub.ApplicationCore**: Contains shared entities and business logic
  - `Entities/Identity/StoryUser.cs`: User entity
  - `Entities/Identity/StoryRole.cs`: Role entity
  - `Configuration/RoleInitializer.cs`: Role seeding logic
- **WibuHub.Common**: Contains constants including role names
- **WibuHub.DataLayer**: Database context and migrations

### Key Features

1. **Role-Based Access Control**: Different roles have access to different features
2. **User Type Separation**: Admin and Customer users are distinguished by `UserType` property
3. **Automatic Role Initialization**: Roles and SuperAdmin account are created on application startup
4. **Identity Integration**: Uses ASP.NET Core Identity for authentication and authorization

## Database Schema

### StoryUser Table
- `Id`: User identifier (string)
- `Email`: User email (unique)
- `FullName`: User's full name
- `Avatar`: Avatar image path
- `UserType`: "Admin" or "Customer"
- `CreatedAt`: Account creation timestamp
- Plus standard Identity fields (UserName, PasswordHash, etc.)

### StoryRole Table
- `Id`: Role identifier (string)
- `Name`: Role name
- `Description`: Role description
- Plus standard Identity fields

## Usage

### Admin Registration
1. Navigate to `/Identity/Account/Register` in Admin portal
2. Fill in email, password, and full name
3. User is automatically assigned "Admin" role
4. User type is set to "Admin"

### Customer Registration
1. Navigate to `/Identity/Account/Register` in Customer portal
2. Fill in email, password, and full name
3. User is automatically assigned "Customer" role
4. User type is set to "Customer"

### Login
Both Admin and Customer portals have `/Identity/Account/Login` endpoints

### Logout
Both portals have `/Identity/Account/Logout` endpoints

## Adding Role-Based Authorization

To restrict access to specific roles, use the `[Authorize]` attribute:

```csharp
// Require any authenticated user
[Authorize]
public IActionResult Index()
{
    return View();
}

// Require specific role
[Authorize(Roles = "SuperAdmin")]
public IActionResult AdminOnly()
{
    return View();
}

// Require one of multiple roles
[Authorize(Roles = "SuperAdmin,Admin,ContentManager")]
public IActionResult ManageContent()
{
    return View();
}
```

## Database Migration

A migration has been created to add the new fields to the StoryUser table:
- Migration name: `AddUserTypeAndCreatedAt`
- Location: `WibuHub.DataLayer/MigrationsIdentity/`

To apply the migration, run:
```bash
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
```

## Configuration

Connection strings are configured in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "StoryConnection": "...",
    "StoryIdentityConnection": "..."
  }
}
```

Both Admin and Customer projects use the same Identity database (`StoryIdentityConnection`).
