using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class SurveyResponseSetConfiguration : IEntityTypeConfiguration<SurveyResponseSet>
{
	public void Configure(EntityTypeBuilder<SurveyResponseSet> builder)
	{
		// Table
		builder.ToTable("SurveyResponseSets");

		// Keys
		builder.HasKey(v => v.SurveyResponseSetId);
		builder.Property(v => v.SurveyResponseSetId).ValueGeneratedOnAdd();

		// Properties
		builder.Property(v => v.SurveyId).IsRequired();
		builder.Property(v => v.CustomerId).IsRequired();
		builder.Property(v => v.VendorId).IsRequired();
		builder.Property(v => v.StartDate).IsRequired();
		builder.Property(v => v.EndDate);
		builder.Property(v => v.CompletedQuestions).IsRequired().HasDefaultValue(0);
		builder.Property(v => v.TotalQuestions).IsRequired().HasDefaultValue(0);
		builder.Property(v => v.LastQuestionIndex).IsRequired().HasDefaultValue(0);
		builder.Property(v => v.ResponseSetStatus).IsRequired();

		// Enum stored as string in the database
		builder.Property(v => v.ResponseSetStatus)
			.HasConversion<string>();

		// Indexes
		builder.HasIndex(v => v.VendorId);
		builder.HasIndex(v => v.SurveyId);
		builder.HasIndex(v => v.CustomerId);
		builder.HasIndex(v => new { v.VendorId, v.SurveyId, v.CustomerId });

		// Relationships - All set to Restrict to avoid cascade conflicts
		builder.HasOne(e => e.Survey)
			.WithMany()
			.HasForeignKey(e => e.SurveyId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.Customer)
			.WithMany()
			.HasForeignKey(e => e.CustomerId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(e => e.Vendor)
			.WithMany()
			.HasForeignKey(e => e.VendorId)
			.OnDelete(DeleteBehavior.Restrict);

	}
}
