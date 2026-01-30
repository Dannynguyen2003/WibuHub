using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WibuHub.Common.Contants
{
    public static class AppConstants
    {
        // User Types
        public const string UserTypeAdmin = "Admin";
        public const string UserTypeCustomer = "Customer";

        // Admin Roles
        public const string RoleSuperAdmin = "SuperAdmin";
        public const string RoleContentManager = "ContentManager"; // Upload, translate stories
        public const string RoleStoryManager = "StoryManager"; // Manage stories and chapters
        public const string RoleSalesManager = "SalesManager"; // Sell stories, manage orders
        public const string RoleAdmin = "Admin"; // General admin role

        // Customer Role
        public const string RoleCustomer = "Customer";

        // All Admin Roles (for convenience)
        public static readonly string[] AdminRoles = new[]
        {
            RoleSuperAdmin,
            RoleAdmin,
            RoleContentManager,
            RoleStoryManager,
            RoleSalesManager
        };
    }
}
