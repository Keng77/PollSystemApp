using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Infrastructure.Users.Persistence.Configuration
{

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {            
        }
    }

}
