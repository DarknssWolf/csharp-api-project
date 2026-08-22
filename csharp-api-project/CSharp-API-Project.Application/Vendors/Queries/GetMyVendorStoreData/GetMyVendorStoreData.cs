using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Application.Vendors.Exceptions;
using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Domain.Entities;

namespace CSharpAPIProject.Application.Vendors.Queries.GetMyVendorStoreData;

public class GetMyVendorStoreDataQuery : IRequest<VendorStoreDataDto>
{
    public int VendorId { get; set; }
}

public class GetMyVendorStoreDataQueryHandler(
    IAuthHelper authHelper,
    IApplicationDbContext context,
    ITenantResolver tenantResolver,
    IMapper mapper,
    ISettingsService settingsService
) : IRequestHandler<GetMyVendorStoreDataQuery, VendorStoreDataDto>
{
    public async Task<VendorStoreDataDto> Handle(GetMyVendorStoreDataQuery request, CancellationToken cancellationToken)
    {
        int vendorId;
        if (await authHelper.LoggedIn_IsTICUser()) vendorId = request.VendorId;
        else vendorId = tenantResolver.ResolveTenantId();

        Vendor? vendor;
        VendorSettings vendorSettings;

        if (await authHelper.LoggedIn_IsTICUser() && vendorId == 0)
        {
            vendor = new Vendor
            {
                VendorId = 0,
                Name = "Administrator Vendor",
                DecimalRounding = 2
            };
            vendorSettings = new VendorSettings
            {
                ClaimPeriodDays = 30,
                PointsName = "ADMIN PTS",
                PointsAbbreviation = "APS"
            };
        }
        else
        {
            vendor = await context.Vendors.Where(v => v.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
            if (vendor == null)
                throw new VendorNotFoundException(vendorId);

            vendorSettings = settingsService.LoadSetting<VendorSettings>();
        }

        var mappedWithVendorData = mapper.Map<VendorStoreDataDto>(vendor);
        var mappedWithSettingData = mapper.Map<VendorStoreDataDto>(vendorSettings);

        return mappedWithSettingData;
    }
}