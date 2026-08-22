using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Infrastructure.Data.Seeders;
public class VoteSeeder(IApplicationDbContext context, string adminUserId)
{
    public async Task SeedDefaultVoteDataAsync()
    {
        var vendor = context.Vendors.FirstOrDefault(v => v.Email == "vendor@localhost.com");
        if (vendor == null) return;

        var voteSet = new VoteSet
        {
            VendorId = vendor.VendorId,
            Title = "Best Improvement Idea",
            Code = "VOTE001",
            StartOn = DateTime.UtcNow.AddDays(-7),
            EndOn = DateTime.UtcNow.AddDays(30),
            AllowMultipleTries = false,
            // The switch that lets participants submit their own vote items:
            WhoCanCreateVoteItemFlag = WhoCanCreateVoteItemFlag.User,
            CreatedBy = adminUserId,
            CreatedOn = DateTimeOffset.UtcNow,
        };
        await context.VoteSets.AddAsync(voteSet);
        await context.SaveChangesAsync(CancellationToken.None);

        context.VoteItems.AddRange(
            new VoteItem
            {
                VoteSetId = voteSet.VoteSetId,
                Title = "Faster support responses",
                Description = "Cut ticket response time in half",
                Status = VoteItemStatus.Approve,
                CreatedBy = adminUserId,
                CreatedOn = DateTimeOffset.UtcNow
            },
            new VoteItem
            {
                VoteSetId = voteSet.VoteSetId,
                Title = "Dark mode",
                Description = "Add a dark theme to the portal",
                Status = VoteItemStatus.Approve,
                CreatedBy = adminUserId,
                CreatedOn = DateTimeOffset.UtcNow
            });
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
