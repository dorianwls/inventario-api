using Microsoft.AspNetCore.Identity;

namespace Inventario.Api.Auth;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
