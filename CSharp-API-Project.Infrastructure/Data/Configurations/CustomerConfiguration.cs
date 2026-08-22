using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Infrastructure.Data.Configurations
{
	public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
	{
		public void Configure(EntityTypeBuilder<Customer> builder)
		{
			// Table
			builder.ToTable("Customer");

			// Keys
			builder.HasKey(v => v.CustomerId);
			builder.Property(v => v.CustomerId).ValueGeneratedOnAdd();

			// Properties
			builder.Property(e => e.FirstName).HasMaxLength(64).IsRequired();
			builder.Property(e => e.LastName).HasMaxLength(64).IsRequired();
			builder.Property(e => e.IDNumber).HasMaxLength(32).IsRequired();
			builder.Property(e => e.Email).HasMaxLength(255).IsRequired();
			builder.Property(e => e.Phone).HasMaxLength(32).IsRequired();
            builder.Property(gi => gi.RecordStatus).IsRequired().HasMaxLength(100);
            builder.Property(gi => gi.RecordDeletedDate);

            builder.OwnsOne(e => e.Address);

			builder.Property(e => e.CustomerData).IsRequired();

			// Relationships
			builder.HasOne(e => e.Vendor)
				.WithMany()
				.HasForeignKey(e => e.VendorId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasOne(e => e.VendorRegion)
				.WithMany()
				.HasForeignKey(e => e.VendorRegionId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
