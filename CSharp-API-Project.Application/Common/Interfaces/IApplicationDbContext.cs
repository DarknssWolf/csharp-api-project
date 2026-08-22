using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CSharpAPIProject.Application.Interfaces.Data;

public interface IApplicationDbContext
{
    
    DbSet<Customer> Customers { get; set; }
	DbSet<Region> Regions { get; set; }
	DbSet<Survey> Surveys { get; set; }
	DbSet<SurveyQuestion> SurveyQuestions { get; set; }
	DbSet<SurveyResponse> SurveyResponses { get; set; }
	DbSet<SurveyResponseSet> SurveyResponseSets { get; set; }
	DbSet<StoredFile> StoredFiles { get; set; }
    DbSet<UserGroup> UserGroups { get; set; }
    DbSet<UserSubGroup> UserSubGroups { get; set; }
	DbSet<Vendor> Vendors { get; set; }
    DbSet<VendorLanguage> VendorLanguages { get; set; }
	DbSet<VoteSet> VoteSets { get; set; }
	DbSet<VoteItem> VoteItems { get; set; }
	DbSet<VoteSubmission> VoteSubmissions { get; set; }

    Task<int> SaveChangesAsync(CancellationToken token);
	int SaveChanges();

	Dictionary<String, Object> GetPrimaryKey<T>(T entity) where T : BaseEntity;
	Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
	Task CommitTransactionAsync();
	Task RollbackTransactionAsync();
} 