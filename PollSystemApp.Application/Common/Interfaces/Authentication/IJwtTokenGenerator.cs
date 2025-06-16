using PollSystemApp.Domain.Users; 
using System.Collections.Generic; 
using System.Security.Claims; 

namespace PollSystemApp.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IList<string> roles, IList<Claim>? additionalClaims = null);
}