using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Vendors.Models;

namespace CSharpAPIProject.Application.Vendors.Queries.GetVendorSettings;

public class GetVendorSettingsQuery : IRequest<VendorSettingsDto> { }

public class GetVendorSettingsQueryHandler(ISettingsService settingsService, IMapper mapper) : IRequestHandler<GetVendorSettingsQuery, VendorSettingsDto>
{
    public async Task<VendorSettingsDto> Handle(GetVendorSettingsQuery request, CancellationToken cancellationToken)
    {
        var Settings = settingsService.LoadSetting<VendorSettings>();

        return mapper.Map<VendorSettingsDto>(Settings);
    }
}