using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Users.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;
using System.Security.Claims;

namespace CSharpAPIProject.Application.Users.Queries.GetPaginatedUserList;

[Authorize(Policy = Policies.CanView_User)]
public class GetPaginatedUserListQuery : IRequest<PaginatedList<UserForVendorDto>>
{
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public string? QueryFilter { get; set; }
	public int VendorId { get; set; } = -1;
}

public class GetPaginatedUserListQueryValidator : AbstractValidator<GetPaginatedUserListQuery>
{
	public GetPaginatedUserListQueryValidator()
	{
		RuleFor(x => x.PageNumber).GreaterThan(0);
		RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(1000);
	}
}

public class GetPaginatedUserListQueryHandler(
    IApplicationDbContext context,
    IIdentityService identityService,
    IGlobalIdentityService globalIdentityService,
    ITenantResolver tenantResolver,
    IAuthHelper authHelper
) : IRequestHandler<GetPaginatedUserListQuery, PaginatedList<UserForVendorDto>>
{
    public async Task<PaginatedList<UserForVendorDto>> Handle(GetPaginatedUserListQuery request, CancellationToken cancellationToken)
    {
        var isPlatformAdmin = await authHelper.LoggedIn_IsPlatformAdmin();

        // Platform admins can filter by any vendor (or none), everyone else is locked to their own tenant.
        var vendorId = isPlatformAdmin ? request.VendorId : tenantResolver.ResolveTenantId();
        var identity = isPlatformAdmin ? globalIdentityService : identityService;

        var (users, totalCount) = await identity.GetUsersForVendorAsync(vendorId, request.QueryFilter, request.PageNumber, request.PageSize);

        // Load the languages for the vendors on this page so user language claims can be resolved.
        var vendorIds = users.Select(u => u.TenantId).Distinct().ToList();
        var languageQuery = context.VendorLanguages.Where(l => vendorIds.Contains(l.VendorId));
        if (isPlatformAdmin)
            languageQuery = languageQuery.IgnoreQueryFilters();

        var vendorLanguages = await languageQuery.AsNoTracking().ToListAsync(cancellationToken);

        var userDtoList = new List<UserForVendorDto>();
        foreach (var account in users)
        {
            var claims = await identity.GetClaimsAsync(account.Id) ?? [];
            var roles = await identity.GetRolesAsync(Guid.Parse(account.Id)) ?? [];

            var statusClaim = claims.FirstOrDefault(c => c.Type == "UserStatus")?.Value;
            var status = Enum.TryParse<UserStatus>(statusClaim, out var parsedStatus) ? parsedStatus : UserStatus.Active;

            // The language claim holds an ISO code; fall back to the vendor's default language.
            var languageCode = claims.FirstOrDefault(c => c.Type == "Language")?.Value;
            var vendorLanguage = vendorLanguages
                .Where(l => l.VendorId == account.TenantId)
                .FirstOrDefault(l => languageCode != null ? l.ISOCode == languageCode : l.IsDefault);

            userDtoList.Add(new UserForVendorDto
            {
                Id = account.Id,
                VendorId = account.TenantId,
                FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? string.Empty,
                Email = account.Email,
                Phone = account.PhoneNumber,
                Status = status,
                Roles = roles,
                UserGroupId = claims.FirstOrDefault(c => c.Type == "UserGroupId")?.Value,
                UserSubGroupId = claims.FirstOrDefault(c => c.Type == "UserSubGroupId")?.Value,
                Language = vendorLanguage == null ? null : new Language
                {
                    Name = vendorLanguage.Name,
                    ISOCode = vendorLanguage.ISOCode,
                    IsDefault = vendorLanguage.IsDefault
                },
                CreationDate = account.CreationDate
            });
        }

        return new PaginatedList<UserForVendorDto>(userDtoList, totalCount, request.PageNumber, request.PageSize);
    }
}
