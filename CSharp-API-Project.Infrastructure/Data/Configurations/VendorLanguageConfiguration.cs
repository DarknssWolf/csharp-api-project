using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;

public class VendorLanguageConfiguration : IEntityTypeConfiguration<VendorLanguage>
{
    public void Configure(EntityTypeBuilder<VendorLanguage> builder)
    {
        // Table
        builder.ToTable("VendorLanguage");

        // PK
        builder.Property(x => x.VendorLanguageId)
            .ValueGeneratedOnAdd();

        builder.HasKey(x => x.VendorLanguageId);

        // FK
        builder.Property(x => x.VendorId)
            .IsRequired();

        // Properties
        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.ISOCode)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false);
    }
}
