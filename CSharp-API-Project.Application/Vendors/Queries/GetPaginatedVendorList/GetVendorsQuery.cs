using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Vendors.Queries.GetPaginatedVendorList;

[Authorize(Policy = Policies.CanView_Vendor)]
public class GetVendorsQuery : IRequest<PaginatedList<VendorDto>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}


public class GetVendorsQueryHandler(
    IApplicationDbContext context,
    IUser user,
    IMapper mapper,
    IIdentityService identityService
) : IRequestHandler<GetVendorsQuery, PaginatedList<VendorDto>>
{
    public async Task<PaginatedList<VendorDto>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Vendor> vendor = null;

        var userRoles = await identityService.GetRolesAsync(user);
        if (userRoles == null)
            throw new UnauthorizedAccessException();

        var isPlatformAdmin = userRoles.Any(r => r == Roles.Administrator);
        var isClientUser = userRoles.Any(r => r == Roles.Vendor);

        if (!isPlatformAdmin && !isClientUser)
            throw new UnauthorizedAccessException();

        if (!isPlatformAdmin)
        {
            var tenantId = await identityService.GetTenantId(user.Id);
            vendor = context.Vendors.Where(x => x.VendorId == tenantId && x.StatusFlag.Status == RecordStatus.Active);
        }
        else
            vendor = context.Vendors.Where(x => x.Email != string.Empty && x.StatusFlag.Status == RecordStatus.Active);

        if (vendor == null)
            throw new VendorListEmptyException();

        return await vendor
            .AsNoTracking()
            .ProjectTo<VendorDto>(mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
