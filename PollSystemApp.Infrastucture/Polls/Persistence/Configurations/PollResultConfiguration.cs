using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Infrastructure.Polls.Persistence.Configurations
{
    public class PollResultConfiguration : IEntityTypeConfiguration<PollResult>
    {
        public void Configure(EntityTypeBuilder<PollResult> builder)
        {
            builder.ToTable("PollResults");
            builder.HasKey(pr => pr.Id);

            builder.HasOne<Poll>()
                .WithMany()
                .HasForeignKey(pr => pr.PollId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(pr => pr.Options)
                .WithOne(ovs => ovs.PollResult)
                .HasForeignKey(ovs => ovs.PollResultId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}