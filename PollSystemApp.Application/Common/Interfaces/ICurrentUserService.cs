using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? UserName { get; }
        string? UserEmail { get; }
        bool IsAuthenticated { get; }
        IEnumerable<Claim>? Claims { get; }
        bool IsInRole(string roleName);
    }
}