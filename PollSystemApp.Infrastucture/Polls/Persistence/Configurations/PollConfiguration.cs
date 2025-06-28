using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Infrastructure.Polls.Persistence.Configurations
{
    public class PollConfiguration : IEntityTypeConfiguration<Poll>
    {
        public void Configure(EntityTypeBuilder<Poll> builder)
        {
            builder.ToTable("Polls");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.HasMany(p => p.Tags)
                .WithMany(t => t.Polls)
                .UsingEntity<Dictionary<string, object>>(
                    "PollTag",
                    j => j
                        .HasOne<Tag>()
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Poll>()
                        .WithMany()
                        .HasForeignKey("PollId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("PollId", "TagId");
                        j.ToTable("PollTags");
                    });
        }
    }
}
