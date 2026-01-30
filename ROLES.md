# WibuHub Role-Based System

## Overview
WibuHub implements a role-based authorization system to differentiate between regular users and system administrators.

## Roles

### Customer Role
- **Description**: Regular users who read and purchase stories
- **Assigned**: Automatically assigned to all newly registered users
- **Permissions**:
  - Read stories
  - Purchase stories (chapters)
  - Follow stories
  - Comment and rate stories
  - View reading history

### Admin Role
- **Description**: System operators who manage the platform
- **Assigned**: Manually assigned by existing administrators
- **Permissions**:
  - All Customer permissions
  - Upload stories
  - Translate stories
  - Manage stories and chapters
  - Manage users
  - Assign/remove roles
  - Access administrative endpoints

## Implementation Details

### Role Constants
Role names are defined as constants in `WibuHub.Common.Constants.AppConstants`:
```csharp
public static class Roles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
}
```

### Role Seeding
Roles are automatically created on application startup via `RoleSeeder.cs`:
- Customer role: "Người dùng đọc truyện và mua truyện"
- Admin role: "Người vận hành hệ thống, quản lý truyện, chapter và người dùng"

### User Registration
New users are automatically assigned the Customer role during registration in `Register.cshtml.cs`.

### Authorization
Controllers use the `[Authorize(Roles = AppConstants.Roles.Admin)]` attribute to restrict access to admin-only endpoints.

## Usage Examples

### Protecting an Endpoint
```csharp
[Route("api/admin/stories")]
[ApiController]
[Authorize(Roles = AppConstants.Roles.Admin)]
public class StoryManagementController : ControllerBase
{
    // Only admins can access these endpoints
}
```

### Assigning Admin Role
Admins can use the CustomerController API endpoint to assign roles:
```
POST /api/admin/customers/{userId}/roles
{
    "roleName": "Admin"
}
```

### Removing a Role
```
DELETE /api/admin/customers/{userId}/roles
{
    "roleName": "Admin"
}
```

## Database Schema
Roles are stored in the `AspNetRoles` table managed by ASP.NET Core Identity.
User-role associations are stored in the `AspNetUserRoles` table.
