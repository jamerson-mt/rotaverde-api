using Microsoft.AspNetCore.Identity;

namespace RotaVerdeAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public int? TurmaId { get; internal set; }

        

    }
}
