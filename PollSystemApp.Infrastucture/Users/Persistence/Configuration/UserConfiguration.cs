using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Domain.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {        
        builder.Property(u => u.RefreshToken).HasMaxLength(256);
        builder.HasIndex(u => u.RefreshToken).IsUnique();

    }
}

