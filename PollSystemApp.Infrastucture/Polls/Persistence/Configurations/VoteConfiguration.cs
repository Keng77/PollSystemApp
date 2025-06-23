using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Infrastructure.Polls.Persistence.Configurations
{
    public class VoteConfiguration : IEntityTypeConfiguration<Vote>
    {
        public void Configure(EntityTypeBuilder<Vote> builder)
        {
            builder.ToTable("Votes");
            builder.HasKey(v => v.Id);

            builder.HasOne<Poll>()
                .WithMany() 
                .HasForeignKey(v => v.PollId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Option>()
                .WithMany() 
                .HasForeignKey(v => v.OptionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
               .WithMany() 
               .HasForeignKey(v => v.UserId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}