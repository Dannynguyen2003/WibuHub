using Microsoft.AspNetCore.Identity;

namespace WibuHub.ApplicationCore.Entities
{
    
    public class StoryRole : IdentityRole
    {
        public string? Description { get; set; } // 
    }
}