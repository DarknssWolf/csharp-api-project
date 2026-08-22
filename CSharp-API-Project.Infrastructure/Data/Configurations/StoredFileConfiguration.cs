using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
	public void Configure(EntityTypeBuilder<StoredFile> builder)
	{
		// Table
		builder.ToTable("StoredFile");

		// Keys
		builder.HasKey(f => f.StoredFileId);
		builder.Property(f => f.StoredFileId).ValueGeneratedOnAdd();

		// Properties
		builder.Property(f => f.FileName).HasMaxLength(255).IsRequired();
		builder.Property(f => f.ContentType).HasMaxLength(255).IsRequired();
		builder.Property(f => f.Length).IsRequired();
		builder.Property(f => f.Content).IsRequired();

		// Indexes
		builder.HasIndex(f => f.VendorId, "IX_StoredFile_VendorId");

		// Relationships
		builder.HasOne(f => f.Vendor)
			.WithMany()
			.HasForeignKey(f => f.VendorId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
