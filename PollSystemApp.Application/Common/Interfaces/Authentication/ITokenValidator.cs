using System.Security.Claims;

namespace PollSystemApp.Application.Common.Interfaces.Authentication
{
    public interface ITokenValidator
    {
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}