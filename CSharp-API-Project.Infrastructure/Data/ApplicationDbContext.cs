using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Identity;
using CSharpAPIProject.Application.Interfaces.Data;

namespace CSharpAPIProject.Infrastructure.Data;

public class ApplicationDbContext(
	DbContextOptions<ApplicationDbContext> options, 
	ITenantResolver tenantResolver
) : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
	private IDbContextTransaction _currentTransaction;

    #region DbSets
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Region> Regions { get; set; }
	public DbSet<Survey> Surveys { get; set; }
	public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
	public DbSet<SurveyResponse> SurveyResponses { get; set; }
	public DbSet<SurveyResponseSet> SurveyResponseSets { get; set; }
	public DbSet<StoredFile> StoredFiles { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<UserSubGroup> UserSubGroups { get; set; }
	public DbSet<Vendor> Vendors { get; set; }
    public DbSet<VendorLanguage> VendorLanguages { get; set; }
	public DbSet<VoteSet> VoteSets { get; set; }
	public DbSet<VoteItem> VoteItems { get; set; }
	public DbSet<VoteSubmission> VoteSubmissions { get; set; }

    #endregion DbSets

    protected override void OnModelCreating(ModelBuilder builder)
	{
        base.OnModelCreating(builder);
		builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        ConfigureAuditableStatusFlag(builder);
        ConfigureMultiTenancy(builder);
        ConfigureTenantQueryFilter(builder);
	}

    /// <summary>
    /// Configures the StatusFlag value object as owned for every auditable entity.
    /// </summary>
    /// <param name="builder"></param>
    private void ConfigureAuditableStatusFlag(ModelBuilder builder)
    {
        builder.Entity<Customer>().OwnsOne(x => x.StatusFlag);
        builder.Entity<Region>().OwnsOne(x => x.StatusFlag);

        builder.Entity<Survey>().OwnsOne(x => x.StatusFlag);
        builder.Entity<SurveyQuestion>().OwnsOne(x => x.StatusFlag);
        builder.Entity<SurveyResponse>().OwnsOne(x => x.StatusFlag);
        builder.Entity<SurveyResponseSet>().OwnsOne(x => x.StatusFlag);
        builder.Entity<StoredFile>().OwnsOne(x => x.StatusFlag);

        builder.Entity<UserGroup>().OwnsOne(x => x.StatusFlag);
        builder.Entity<UserSubGroup>().OwnsOne(x => x.StatusFlag);

        builder.Entity<Vendor>().OwnsOne(x => x.StatusFlag);
        builder.Entity<VendorLanguage>().OwnsOne(x => x.StatusFlag);

        builder.Entity<VoteSet>().OwnsOne(x => x.StatusFlag);
        builder.Entity<VoteItem>().OwnsOne(x => x.StatusFlag);
        builder.Entity<VoteSubmission>().OwnsOne(x => x.StatusFlag);
    }

    #region Helpers

    private String[] GetKeyNames<T>() where T : BaseEntity
    {
        var entityType = this.Model.FindEntityType(typeof(T));
        if(entityType == null) throw new ArgumentException($"Entity type {typeof(T).Name} not found in the model.");

        var key = entityType.FindPrimaryKey();

        if(key == null)
            throw new ArgumentException($"Entity type {typeof(T).Name} does not have a primary key.");

        return key.Properties.Select(p => p.Name).ToArray();
    }

	public Dictionary<String, Object> GetPrimaryKey<T>(T entity) where T : BaseEntity
	{
		Object GetEntityValue(String name) => entity.GetType().GetProperty(name)?.GetValue(entity, null);

		var primaryKeyProperties = GetKeyNames<T>();

		return (from p in primaryKeyProperties
				select new
				{
					Name = p,
					Value = GetEntityValue(p)
				}).ToDictionary(a => a.Name, b => b.Value);
	}

    #endregion Helpers

    #region Multi-Tenancy Configuration

    /// <summary>
    /// Configures the model to support multi-tenancy by ensuring that user entities are unique per tenant.
    /// This method adds the index on the ApplicationUser entity to ensure that the combination of Email and TenantId is unique.
    /// We also modify the default identity indexes on NormalizedUserName and NormalizedEmail to include TenantId.
    /// </summary>
    /// <param name="builder"></param>
    public void ConfigureMultiTenancy(ModelBuilder builder)
    {
        // This allows for multi tenanting
        builder.Entity<ApplicationUser>()
            .HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique();

        //Modify the default identity index on NormalizedUserName to include TenantId
        builder.Entity<ApplicationUser>(entity =>
        {
            var noramlizedUserNameIndex = entity.HasIndex(e => new { e.NormalizedUserName }).Metadata;
            entity.Metadata.RemoveIndex(noramlizedUserNameIndex.Properties);

            entity.HasIndex(a => new { a.NormalizedUserName, a.TenantId }).HasDatabaseName("UserNameIndex").IsUnique();
        });

        //Modify the default identity index on NormalizedUserName to include TenantId
        builder.Entity<ApplicationUser>(entity =>
        {
            var EmailIndex = entity.HasIndex(e => new { e.NormalizedEmail }).Metadata;
            entity.Metadata.RemoveIndex(EmailIndex.Properties);

            entity.HasIndex(a => new { a.NormalizedEmail, a.TenantId }).HasDatabaseName("EmailIndex").IsUnique();
        });
    }

    #endregion Multi-Tenancy Configuration

    #region Query Filters
    protected int GetCurrentTenantId()
    {
        return tenantResolver.ResolveTenantId();
    }

    protected int GetCurrentUserTenantId()
    {
        // If this is an administrator user then we need to get the users tenant Id
        // and not the Tenant Id of the data structure that we are accessing.
        return tenantResolver.ResolveUserTenantId();
    }

    /// <summary>
    /// Applies a query filter to ensure only data relevant to the current tenant is returned.
    /// <para>
    /// <b>IMPORTANT:</b> Understand the distinction between Tenant ID and Tenant User ID.
    /// </para>
    /// 
    /// <para>
    /// <b>TenantId</b>: Identifies the <i>tenant context</i> for the request (e.g., vendor).
    /// <b>TenantUserId</b>: Identifies the <i>user</i> making the request, within the context of a tenant.
    /// </para>
    /// 
    /// <para><b>Example:</b> A admin wants to view survey responses for a specific tenant.</para>
    /// 
    /// <list type="bullet">
    ///   <item><description>X-Tenant-Id = 321 (Target tenant)</description></item>
    ///   <item><description>X-Tenant-User-Id = 0 (admin, not part of tenant's ASP.NET Identity)</description></item>
    /// </list>
    /// 
    /// <para>
    /// <b>Behavior:</b>  
    /// - If authorization is required, CSharpAPIProject reads the user ID from the JWT.  
    /// - It then tries to match it using <c>X-Tenant-User-Id</c>.  
    /// - If this is incorrectly set to the tenant ID, lookup will fail (admin not in that tenant).
    /// </para>
    /// 
    /// <para>
    /// <b>Fallback:</b> If <c>X-Tenant-User-Id</c> is omitted, it defaults to <c>X-Tenant-Id</c>,  
    /// which may lead to incorrect user resolution in cross-tenant scenarios.
    /// </para>
    /// 
    /// <para><b>TL;DR:</b></para>
    /// <list type="bullet">
    ///   <item><description><c>TenantId</c> = the tenant whose data is being accessed.</description></item>
    ///   <item><description><c>TenantUserId</c> = the actual user making the request.</description></item>
    /// </list>
    /// </summary>
    /// <param name="builder">The model builder used to apply the tenant filter.</param>
    public void ConfigureTenantQueryFilter(ModelBuilder builder)
    {
        // Secure the User DataSet with the Tenant
        builder.Entity<ApplicationUser>().HasQueryFilter(u => u.TenantId == GetCurrentUserTenantId());

        builder.Entity<Customer>().HasQueryFilter(c => c.VendorId == GetCurrentTenantId());

        builder.Entity<Region>().HasQueryFilter(c => c.VendorId == GetCurrentTenantId());
        builder.Entity<Survey>().HasQueryFilter(s => s.VendorId == GetCurrentTenantId());
        builder.Entity<SurveyQuestion>().HasQueryFilter(sq => sq.Survey.VendorId == GetCurrentTenantId());
        builder.Entity<SurveyResponse>().HasQueryFilter(sr => sr.VendorId == GetCurrentTenantId());
        builder.Entity<SurveyResponseSet>().HasQueryFilter(srs => srs.VendorId == GetCurrentTenantId());
        builder.Entity<StoredFile>().HasQueryFilter(f => f.VendorId == GetCurrentTenantId());
        builder.Entity<UserGroup>().HasQueryFilter(ug => ug.VendorId == GetCurrentTenantId());
        builder.Entity<UserSubGroup>().HasQueryFilter(usg => usg.VendorId == GetCurrentTenantId());
        builder.Entity<VendorLanguage>().HasQueryFilter(s => s.VendorId == GetCurrentTenantId());
        builder.Entity<VoteSet>().HasQueryFilter(vs => vs.VendorId == GetCurrentTenantId());
        builder.Entity<VoteItem>().HasQueryFilter(vi => vi.VoteSet.VendorId == GetCurrentTenantId());
        builder.Entity<VoteSubmission>().HasQueryFilter(vs => vs.VoteItem.VoteSet.VendorId == GetCurrentTenantId());
    }

	#endregion Query Filters

	#region Transactions

	// Begin transaction
	public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
	{
		if (_currentTransaction != null)
		{
			return _currentTransaction;
		}

		_currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
		return _currentTransaction;
	}

	// Commit transaction
	public async Task CommitTransactionAsync()
	{
		try
		{
			if (_currentTransaction != null)
			{
				await _currentTransaction.CommitAsync();
			}
		}
		finally
		{
			if (_currentTransaction != null)
			{
				await _currentTransaction.DisposeAsync();
				_currentTransaction = null;
			}
		}
	}

	// Rollback transaction
	public async Task RollbackTransactionAsync()
	{
		try
		{
			if (_currentTransaction != null)
			{
				await _currentTransaction.RollbackAsync();
			}
		}
		finally
		{
			if (_currentTransaction != null)
			{
				await _currentTransaction.DisposeAsync();
				_currentTransaction = null;
			}
		}
	}

	#endregion Transactions
}
