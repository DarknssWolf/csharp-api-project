using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations
{
	public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
	{
		public void Configure(EntityTypeBuilder<SurveyResponse> builder)
		{
			// Table
			builder.ToTable("SurveyResponse");

			// Keys
			builder.HasKey(v => v.SurveyResponseId);
			builder.Property(v => v.SurveyResponseId).ValueGeneratedOnAdd();

			// Properties
			builder.Property(v => v.Response).HasMaxLength(1000).IsRequired();
			builder.Property(v => v.VendorId).IsRequired();
			builder.Property(v => v.CustomerId).IsRequired();

			// Relationships
			builder.HasOne(e => e.SurveyQuestion)
				.WithMany(q =>q.Responses)
				.HasForeignKey(e => e.SurveyQuestionId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
