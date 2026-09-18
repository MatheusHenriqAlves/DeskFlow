using System.Security.Claims;
using DeskFlow.Domain.Models.Enums;

namespace DeskFlow.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identifier claim is missing.");
        return Guid.Parse(value);
    }

    public static UserRole GetUserRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("Role claim is missing.");
        return Enum.Parse<UserRole>(value);
    }
}
