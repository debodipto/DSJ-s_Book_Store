using System.Security.Claims;
using DSJsBookStore.Constants;
using Microsoft.AspNetCore.Authentication;

namespace DSJsBookStore.Shared;

public class AdminClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        var email = principal.Identity?.Name;
        var isAdminEmail = string.Equals(email, "admin@gmail.com", StringComparison.OrdinalIgnoreCase);
        var hasAdminRole = principal.IsInRole(nameof(Roles.Admin));

        if (isAdminEmail && !hasAdminRole)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, nameof(Roles.Admin)));
        }

        return Task.FromResult(principal);
    }
}
