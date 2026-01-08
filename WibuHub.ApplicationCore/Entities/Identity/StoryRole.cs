using Microsoft.AspNetCore.Identity;

namespace WibuHub.ApplicationCore.Entities.Identity
{
    
    public class StoryRole : IdentityRole
    {
        public string? Description { get; set; } // 
    }
}