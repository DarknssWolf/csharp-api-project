using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
	public void Configure(EntityTypeBuilder<Region> builder)
	{
		// Table
		builder.ToTable("Region");

		// PK
		builder.HasKey(e => e.RegionId);
		builder.Property(e => e.RegionId).ValueGeneratedOnAdd();

		// Properties
		builder.Property(e => e.RegionCode).IsRequired().HasMaxLength(10);
		builder.Property(e => e.RegionName).IsRequired().HasMaxLength(100);

		// Indexes
		builder.HasIndex(e => new { e.VendorId, e.RegionCode }).IsUnique();

		// Relationships
		builder.HasOne(e => e.Vendor)
			.WithMany()
			.OnDelete(DeleteBehavior.Restrict);
	}
}
