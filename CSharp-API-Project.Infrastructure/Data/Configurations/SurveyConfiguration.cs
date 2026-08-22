using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
	public void Configure(EntityTypeBuilder<Survey> builder)
	{
		// Table
		builder.ToTable("Survey");

		// Keys
		builder.HasKey(v => v.SurveyId);
		builder.Property(v => v.SurveyId).ValueGeneratedOnAdd();

		// Properties
		builder.Property(v => v.Title).HasMaxLength(100).IsRequired();
		builder.Property(v => v.Code).HasMaxLength(10).IsRequired();
		builder.Property(v => v.StartOn).IsRequired();
		builder.Property(v => v.EndOn).IsRequired();
		builder.Property(v => v.CanEarnPoints).HasDefaultValue(false);
		builder.Property(v => v.AllowMultipleTries).HasDefaultValue(false);
		builder.Property(v => v.MultipleTriesLimit).HasDefaultValue(1);
		builder.Property(gi => gi.RecordStatus).IsRequired().HasMaxLength(100);
		builder.Property(gi => gi.RecordDeletedDate);

		// Relationships
		builder.HasMany(e => e.SurveyQuestions)
			.WithOne(q => q.Survey)
			.HasForeignKey(q => q.SurveyId);

		builder.HasOne(e => e.Vendor)
			.WithMany()
			.HasForeignKey(e => e.VendorId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
