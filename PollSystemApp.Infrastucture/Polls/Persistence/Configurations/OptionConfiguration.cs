using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Infrastructure.Polls.Persistence.Configurations
{
    public class OptionConfiguration : IEntityTypeConfiguration<Option>
    {
        public void Configure(EntityTypeBuilder<Option> builder)
        {
            builder.ToTable("Options");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.Text)
                .IsRequired()
                .HasMaxLength(500);
        }
    }
}