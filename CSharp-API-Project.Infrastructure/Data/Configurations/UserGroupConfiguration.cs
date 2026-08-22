using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpAPIProject.Infrastructure.Data.Configurations;
public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        // Table
        builder.ToTable("UserGroup");
        // Keys
        builder.HasKey(g => g.GroupId);
        builder.Property(g => g.GroupId).ValueGeneratedOnAdd();
        // Properties
        builder.Property(g => g.Name).IsRequired();
        builder.Property(g => g.Description).IsRequired(false);
        builder.Property(g => g.OrderNumber).IsRequired();
        builder.Property(v => v.VendorId).IsRequired().HasDefaultValue(0);
    }
}
