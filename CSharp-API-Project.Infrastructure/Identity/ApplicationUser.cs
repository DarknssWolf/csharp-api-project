using Microsoft.AspNetCore.Identity;

namespace CSharpAPIProject.Application.Identity;

public class ApplicationUser : IdentityUser
{
    public int TenantId { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}
