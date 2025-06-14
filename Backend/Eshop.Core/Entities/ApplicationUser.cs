using Microsoft.AspNetCore.Identity;
using Eshop.Core.Enums;

namespace Eshop.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string? Address { get; set; }
        public string SecurityQuestion { get; set; }
        public bool IsBlocked { get; set; } = false;
    }
}
