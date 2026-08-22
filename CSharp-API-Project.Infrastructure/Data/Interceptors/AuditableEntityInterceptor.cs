using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;
using CSharpAPIProject.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Application.Interfaces.Entities;
using System.Collections.Generic;

namespace CSharpAPIProject.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
	private readonly IUser _user;
	private readonly TimeProvider _dateTime;

	public AuditableEntityInterceptor(IUser user, TimeProvider dateTime)
	{
		_user = user;
		_dateTime = dateTime;
	}

	public override InterceptionResult<int> SavingChanges(
		DbContextEventData eventData,
		InterceptionResult<int> result
	)
	{
		SetEntityStatusFlags(eventData.Context);

		return base.SavingChanges(eventData, result);
	}

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default
	)
	{
		SetEntityStatusFlags(eventData.Context);

		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default(CancellationToken))
    {
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public void SetEntityStatusFlags(DbContext? context)
	{
		if (context == null)
			return;

		foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
		{
			if (
				entry.State is EntityState.Added or EntityState.Modified
				|| entry.HasChangedOwnedEntities()
			)
			{
				var utcNow = _dateTime.GetUtcNow();
				if (entry.State == EntityState.Added)
				{
					if(!string.IsNullOrEmpty(_user.Id)) entry.Entity.CreatedBy = _user.Id;
					entry.Entity.CreatedOn = utcNow;
				}

				entry.Entity.LastModifiedBy = _user.Id;
				entry.Entity.LastModifiedOn = utcNow;

                if(entry.State is EntityState.Added || entry.Entity.StatusFlag.Status == null)
                {
                    entry.Entity.StatusFlag = new EntityStatus(
                        RecordStatus.Active,
                        _user.Id,
                        utcNow
                    );
                }
            }
		}
	}
}

public static class Extensions
{
	public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
		entry.References.Any(r =>
			r.TargetEntry != null
			&& r.TargetEntry.Metadata.IsOwned()
			&& (
				r.TargetEntry.State == EntityState.Added
				|| r.TargetEntry.State == EntityState.Modified
			)
		);
}
