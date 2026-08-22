using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;
public class UserSubGroupConfiguration : IEntityTypeConfiguration<UserSubGroup>
{
    public void Configure(EntityTypeBuilder<UserSubGroup> builder)
    {
        // Table
        builder.ToTable("UserSubGroup");

        // Keys
        builder.HasKey(g => g.SubGroupId);
        builder.Property(g => g.SubGroupId).ValueGeneratedOnAdd();

        // Properties
        builder.Property(g => g.Name).IsRequired();
        builder.Property(g => g.Description).IsRequired(false);
        builder.Property(g => g.OrderNumber).IsRequired();

        builder.Property(v => v.VendorId).IsRequired().HasDefaultValue(0);

        // Relationships
        builder.HasOne(g => g.UserGroup)
            .WithMany() 
            .HasForeignKey(g => g.GroupId)  
            .OnDelete(DeleteBehavior.Restrict);
    }
}
