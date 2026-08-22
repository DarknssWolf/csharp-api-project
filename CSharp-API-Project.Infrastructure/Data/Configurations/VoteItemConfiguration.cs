using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class VoteItemConfiguration : IEntityTypeConfiguration<VoteItem>
{
    public void Configure(EntityTypeBuilder<VoteItem> builder)
    {
        // Table
        builder.ToTable("VoteItem");

        // Keys
        builder.HasKey(v => v.VoteItemId);
        builder.Property(v => v.VoteItemId).ValueGeneratedOnAdd();

        // Properties
        builder.Property(v => v.Title).IsRequired();
        builder.Property(v => v.Description).IsRequired();
        builder.Property(v => v.VoteItemData);
        builder.Property(v => v.Status).IsRequired();
        builder.Property(v => v.VoteItemStatusReason).HasMaxLength(1000);

		// Enum stored as string in the database
		builder.Property(q => q.Status)
			.HasConversion<string>();

		// Relationships
		builder.HasOne(e => e.VoteSet)
            .WithMany(e => e.VoteItems)
            .HasForeignKey(e => e.VoteSetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
