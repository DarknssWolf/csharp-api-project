using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Vendors.Queries.GetVendorQuery;

[Authorize(Roles = Roles.Vendor)]
[Authorize(Roles = Roles.Administrator)]
[Authorize(Roles = Roles.TICAdministrator)]
[Authorize(Roles = Roles.TICSuperAdmin)]
public class GetVendorQuery : IRequest<VendorDto>
{
    public int VendorId { get; set; } = -1;
}

public class GetVendorQueryHandler(
    IApplicationDbContext context,
    IUser user,
    IMapper mapper,
    IIdentityService identityService,
    IAuthHelper authHelper
) : IRequestHandler<GetVendorQuery, VendorDto>
{
    public async Task<VendorDto> Handle(GetVendorQuery request, CancellationToken cancellationToken)
    {
        var isTICUser = await authHelper.LoggedIn_IsTICUser();

        Vendor vendor;

        if (!isTICUser)
            vendor = await context
                .Vendors
                .IgnoreQueryFilters()
                .Where(x => x.VendorId == user.VendorId && x.StatusFlag.Status == RecordStatus.Active)
                .FirstOrDefaultAsync(cancellationToken) ?? throw new VendorNotFoundException(request.VendorId);
        else 
            vendor = await context.Vendors.Where(x => x.VendorId == request.VendorId && x.StatusFlag.Status == RecordStatus.Active).FirstOrDefaultAsync(cancellationToken) ?? throw new VendorNotFoundException(request.VendorId);

        if (vendor == null)
            throw new VendorNotFoundException(user.VendorId);
        return mapper.Map<VendorDto>(vendor);
    }
}
