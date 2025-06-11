using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Domain.Users;

public class User: IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }

    public bool IasActive { get; set; }

    
}
