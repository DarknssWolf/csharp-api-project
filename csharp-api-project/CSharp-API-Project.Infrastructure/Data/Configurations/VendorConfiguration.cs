using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Infrastructure.Data.Configurations
{
	public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
	{
		public void Configure(EntityTypeBuilder<Vendor> builder)
		{
			// Table
			builder.ToTable("Vendor");

			// Keys
			builder.HasKey(v => v.VendorId);
			builder.Property(v => v.VendorId).ValueGeneratedOnAdd();

			// Properties
			builder.Property(v => v.Code).HasMaxLength(50).IsRequired();
			builder.Property(v => v.Name).HasMaxLength(100).IsRequired();
			
			builder.Property(s => s.WebhookUrl).HasMaxLength(255).IsRequired();
			builder.Property(s => s.WebhookSecret).HasMaxLength(32).IsRequired();

			builder.Property(s => s.ContactName).HasMaxLength(100).IsRequired();
			builder.Property(s => s.ContactEmail).HasMaxLength(100).IsRequired();
			builder.Property(s => s.FinanceEmail).HasMaxLength(100).IsRequired();

            // Indexes
            builder.HasIndex(v => v.Code, "IX_Vendor_Code").IsUnique();

			// Relationships
            builder.HasMany(v => v.Languages)
                .WithOne(x => x.Vendor)
                .HasForeignKey(x => x.VendorId);
		}
	}
}
