using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Infrastructure.Polls.Persistence.Configurations
{
    public class OptionVoteSummaryConfiguration : IEntityTypeConfiguration<OptionVoteSummary>
    {
        public void Configure(EntityTypeBuilder<OptionVoteSummary> builder)
        {
            builder.ToTable("OptionVoteSummaries");
            builder.HasKey(ovs => ovs.Id);


            builder.HasOne<Option>()
                .WithMany()
                .HasForeignKey(ovs => ovs.OptionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}