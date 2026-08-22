using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class VoteSetConfiguration : IEntityTypeConfiguration<VoteSet>
{
    public void Configure(EntityTypeBuilder<VoteSet> builder)
    {
        // Table
        builder.ToTable("VoteSet");

        // Keys
        builder.HasKey(v => v.VoteSetId);
        builder.Property(v => v.VoteSetId).ValueGeneratedOnAdd();

        // Properties
        builder.Property(v => v.Title).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Code).HasMaxLength(10).IsRequired();
        builder.Property(v => v.StartOn).IsRequired();
        builder.Property(v => v.EndOn).IsRequired();
        builder.Property(v => v.CanEarnPoints).HasDefaultValue(false);
        builder.Property(v => v.AllowMultipleTries).HasDefaultValue(false);
        builder.Property(v => v.AllowMultipleItems).HasDefaultValue(false);
		builder.Property(v => v.EmailNotification).HasMaxLength(250);
        builder.Property(v => v.WhoCanCreateVoteItemFlag).IsRequired();

		// Enum stored as string in the database
		builder.Property(q => q.WhoCanCreateVoteItemFlag)
			.HasConversion<string>();

		// Relationships
		builder.HasMany(e => e.VoteItems)
            .WithOne(e => e.VoteSet)
            .HasForeignKey(e => e.VoteSetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Vendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
