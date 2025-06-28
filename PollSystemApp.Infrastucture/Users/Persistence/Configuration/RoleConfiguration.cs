using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Infrastructure.Users.Persistence.Configuration
{

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasData(
              new Role
              {
                  Id = new Guid("11111111-49b6-410c-bc78-2d54a9991870"),
                  Name = "Admin",
                  NormalizedName = "ADMIN"
              },
              new Role
              {
                  Id = new Guid("11111112-49b6-410c-bc78-2d54a9991870"),
                  Name = "User",
                  NormalizedName = "USER"
              }
            );

        }
    }

}
