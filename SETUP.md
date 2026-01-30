# Setup Instructions for Authentication System

## Prerequisites
- .NET 8.0 SDK
- SQL Server
- dotnet-ef tools (already installed in this project)

## Database Setup

### 1. Update Connection Strings
The connection strings are already configured in `appsettings.json` files:
- Admin: `WibuHub/appsettings.json`
- Customer: `WibuHub.MVC.Customer/appsettings.json`

Both projects use:
- `StoryConnection`: Main application database
- `StoryIdentityConnection`: Identity and authentication database

### 2. Apply Migrations

#### For Identity Database (Authentication):
```bash
cd /home/runner/work/WibuHub/WibuHub
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryIdentityDbContext
```

This will create/update:
- AspNetUsers table with UserType and CreatedAt fields
- AspNetRoles table
- All other Identity-related tables

#### For Main Application Database:
```bash
cd /home/runner/work/WibuHub/WibuHub
dotnet ef database update --project WibuHub.DataLayer --startup-project WibuHub --context StoryDbContext
```

## Running the Applications

### Admin Portal
```bash
cd /home/runner/work/WibuHub/WibuHub/WibuHub
dotnet run
```
Default URL: https://localhost:5001 (or http://localhost:5000)

### Customer Portal
```bash
cd /home/runner/work/WibuHub/WibuHub/WibuHub.MVC.Customer
dotnet run
```
Default URL: https://localhost:5003 (or http://localhost:5002)

## Default SuperAdmin Account

On first run, a SuperAdmin account is automatically created:
- **Email**: superadmin@wibuhub.com
- **Password**: SuperAdmin@123

**IMPORTANT**: Change this password immediately in production!

## Testing the System

### 1. Test Admin Registration
1. Navigate to Admin portal
2. Go to `/Identity/Account/Register`
3. Register a new admin account
4. User will automatically be assigned "Admin" role

### 2. Test Customer Registration
1. Navigate to Customer portal
2. Go to `/Identity/Account/Register`
3. Register a new customer account
4. User will automatically be assigned "Customer" role

### 3. Test Login/Logout
1. Use `/Identity/Account/Login` to login
2. Use `/Identity/Account/Logout` to logout
3. Try accessing protected pages to verify authorization

### 4. Test Role-Based Access
- Try accessing Stories controller (requires Admin roles)
- Try accessing Reports controller (requires SuperAdmin or Admin)
- Verify customers cannot access admin pages

## Troubleshooting

### Database Connection Issues
If you encounter connection errors:
1. Verify SQL Server is running
2. Check connection strings in appsettings.json
3. Ensure SQL Server authentication is properly configured

### Migration Issues
If migrations fail:
1. Drop the databases and recreate them
2. Ensure no other application is using the databases
3. Run migrations again

### Email Confirmation
Currently, email confirmation is required but not implemented. To bypass:
1. Set user's `EmailConfirmed` to `true` in database
2. Or modify `Program.cs` to set `options.SignIn.RequireConfirmedEmail = false`

## Next Steps

1. **Configure Email Service**: Implement email sending for account confirmation
2. **Add Authorization Policies**: Create custom policies for fine-grained access control
3. **Implement Password Reset**: Add forgot password functionality
4. **Add Profile Management**: Allow users to update their profiles
5. **Implement Two-Factor Authentication**: Add 2FA for enhanced security
6. **Add Activity Logging**: Track user actions for auditing
