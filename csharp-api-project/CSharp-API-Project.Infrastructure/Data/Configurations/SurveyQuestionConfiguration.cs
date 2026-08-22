using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
	public void Configure(EntityTypeBuilder<SurveyQuestion> builder)
	{
		// Table
		builder.ToTable("SurveyQuestion");

		// Keys
		builder.HasKey(v => v.SurveyQuestionId);
		builder.Property(v => v.SurveyQuestionId).ValueGeneratedOnAdd();

		// Properties
		builder.Property(v => v.Prompt).HasMaxLength(100).IsRequired();
		builder.Property(v => v.Type).IsRequired();
		builder.Property(v => v.ExpectedAnswers).HasMaxLength(1000).IsRequired();
		builder.Property(v => v.AllowMultipleResponses).IsRequired();
		builder.Property(v => v.Options).HasMaxLength(1000).IsRequired();
		builder.Property(e => e.OrderNumber).HasDefaultValue(1);
		builder.Property(e => e.IsRequired).HasDefaultValue(false);

		//unique
		builder.HasIndex(q => new { q.SurveyId, q.QuestionId, q.ParentQuestionId });

		builder.HasIndex(q => q.SurveyId);

		// Enum stored as string in the database
		builder.Property(q => q.Type)
			.HasConversion<string>();

		builder.HasOne(e => e.Survey)
			.WithMany(e => e.SurveyQuestions)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasMany(e => e.Responses)
			.WithOne(e => e.SurveyQuestion)
			.OnDelete(DeleteBehavior.Restrict); // Optional: Cascade delete
	}
}
