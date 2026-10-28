using Microsoft.AspNetCore.Identity;

namespace Xpress.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
}
