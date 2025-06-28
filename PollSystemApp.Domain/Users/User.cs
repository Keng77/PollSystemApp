using Microsoft.AspNetCore.Identity;

namespace PollSystemApp.Domain.Users;

public class User : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}
