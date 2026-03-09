using System.Collections.Generic;

namespace WibuHub.MVC.ViewModels
{
    public class UserRoleListVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class UserRoleEditVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> AllRoles { get; set; } = new List<string>();
        public IList<string> SelectedRoles { get; set; } = new List<string>();
        public bool IsSuperAdmin { get; set; }
    }
}
