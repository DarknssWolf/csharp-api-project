using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class VoteSubmissionConfiguration : IEntityTypeConfiguration<VoteSubmission>
{
    public void Configure(EntityTypeBuilder<VoteSubmission> builder)
    {
        // Table
        builder.ToTable("VoteSubmission");

        // Keys
        builder.HasKey(v => v.VoteSubmissionId);
        builder.Property(v => v.VoteSubmissionId).ValueGeneratedOnAdd();

        // Relationships
        builder.HasOne(e => e.VoteItem)
            .WithMany(e => e.VoteSubmissions)
            .HasForeignKey(e => e.VoteItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
