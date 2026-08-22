using System.Data;
using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.CloudStorage;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Interfaces.HelperServices;
using CSharpAPIProject.Application.Common.Interfaces.OpenXML;
using CSharpAPIProject.Application.Models.Settings;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.Customers.Exceptions;
using CSharpAPIProject.Application.Customers.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;
using DocumentFormat.OpenXml.Packaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Z.EntityFramework.Plus;

namespace CSharpAPIProject.Application.Customers.Queries.ExportCustomerList;

[Authorize(Roles = Roles.Administrator)]
[Authorize(Roles = Roles.Vendor)]
[Authorize(Roles = Roles.VendorApi)]
[Authorize(Roles = Roles.TICSuperAdmin)]
[Authorize(Roles = Roles.TICAdministrator)]
[Authorize(Roles = Roles.ClientAdmin)]
[Authorize(Roles = Roles.ClientManager)]
public class ExportCustomerListQuery : IRequest<ExportCustomerListQueryResponse>
{
}

public class ExportCustomerListQueryResponse
{
	public bool Success { get; set; } = false;
	public string Message { get; set; } = string.Empty;
	public string GalleryImageId { get; set; } = string.Empty;
}

public class ExportCustomerListQueryHandler(
    IApplicationDbContext context, 
    IUser user, 
    IMapper mapper, 
    IIdentityService identityService, 
    ITenantResolver tenantResolver, 
    ISettingsService settingsService, 
    IOpenXMLService openXmlService, 
    ICloudFileStorage cloudFileStorage,
    IAuthHelper authHelper
) : IRequestHandler<ExportCustomerListQuery, ExportCustomerListQueryResponse>
{

    public async Task<ExportCustomerListQueryResponse> Handle(ExportCustomerListQuery request, CancellationToken cancellationToken)
    {
        // Get Customer List
        var vendorId = tenantResolver.ResolveTenantId();

        var query = context.Customers.Where(c => c.VendorId == vendorId);

        //Determine the purpose of this code, it's stopping exports from getting all values.
        //if (await identityService.IsInRoleAsync(user.Id, Roles.Customer))
        //{
        //	var customerEmail = await identityService.GetUserEmailAsync(user.Id);
        //	query = query.Where(c => c.Email == customerEmail && c.VendorId == vendorId);
        //}

        var isTICUser = await authHelper.LoggedIn_IsTICUser();
        var isClientUser = await authHelper.LoggedIn_IsClientAdmin();

        if (!isTICUser && !isClientUser)
		{
			var customerEmail = await identityService.GetUserEmailAsync(user.Id);
			query = query.Where(c => c.Email == customerEmail && c.VendorId == vendorId);
		}

        var customerList = await query
            .AsNoTracking()
            .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        if (!customerList.Any())
        {
            return new ExportCustomerListQueryResponse
            {
                Success = false,
                Message = "No participants found to export."
            };
        }

        // Create export
        var vendorSettings = settingsService.LoadSetting<VendorSettings>();
        var exportGalleryId = vendorSettings.ExportGalleryId;

        // Create Excel workbook
        var workbook = openXmlService.CreateExcelWorkbook();

        openXmlService.AddSheet(workbook, "Customers");

        var dataSet = new DataSet();
        var dataTable = new DataTable("Customers");
        dataTable.Columns.Add("FirstName", typeof(string));
        dataTable.Columns.Add("LastName", typeof(string));
        dataTable.Columns.Add("Company", typeof(string));
        dataTable.Columns.Add("Phone", typeof(string));
        dataTable.Columns.Add("ID", typeof(string));
        dataTable.Columns.Add("Email", typeof(string));
        dataTable.Columns.Add("Province", typeof(string));
        dataTable.Columns.Add("City", typeof(string));
        dataTable.Columns.Add("AddressLine1", typeof(string));
        dataTable.Columns.Add("AddressLine2", typeof(string));
        dataTable.Columns.Add("PostalCode", typeof(string));
        dataTable.Columns.Add("PreferredSports", typeof(string));

        foreach (var customer in customerList)
        {
            var (companyName, preferredSports) = ExtractFieldsFromCustomerData(customer.CustomerData);

            dataTable.Rows.Add(
                customer.FirstName,
                customer.LastName,
                companyName,
                customer.Phone,
                customer.IDNumber,
                customer.Email,
                customer.Address?.Province,
                customer.Address?.City,
                customer.Address?.Street,
                customer.Address?.Street2,
                customer.Address?.PostalCode,
                preferredSports != null ? string.Join("/", preferredSports) : string.Empty
            );
        }

        dataSet.Tables.Add(dataTable);
        openXmlService.AddDataSetToSheet(workbook, dataSet, "Customers");

        // Save the workbook to a file
        var exportFileName = $"CustomerList_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        var exportFilePath = Path.Combine(Path.GetTempPath(), exportFileName);

        using (var fileStream = new FileStream(exportFilePath, FileMode.Create, FileAccess.ReadWrite))
        {
            // SaveAs is deprecated, as per API docs, use clone instead as it does the same thing.
            // https://learn.microsoft.com/en-us/dotnet/api/documentformat.openxml.packaging.openxmlpackage.saveas?view=openxml-2.20.0&viewFallbackFrom=openxml-3.0.1#documentformat-openxml-packaging-openxmlpackage-saveas(system-string)
            workbook.Clone(fileStream);
        }

        workbook.Dispose();

        // Upload file to Azure Cloud Storage
        var vendor = context.Vendors.Where(v => v.VendorId == vendorId).FutureValue();
        if (vendor.Value == null)
            throw new VendorNotFoundException(vendorId);

        var gallery = context.Galleries.Where(g => g.GalleryId == exportGalleryId).FutureValue();

        var containerName = cloudFileStorage.GetNormalizedContainerName(vendor.Value.Name);

        var uploadPath = await cloudFileStorage.UploadFileAsync(containerName, gallery.Value.Code, exportFileName, exportFilePath);

        // Save metadata to database and get Gallery Image ID
        string destUploadPath = $"{containerName}/{gallery.Value.Code}/{exportFileName}";
        var galleryImage = new GalleryImage
        {
            GalleryId = exportGalleryId,
            Name = exportFileName,
            ImagePath = destUploadPath,
            Status = GalleryImageStatus.Approved,
            Description = "Exported participant list"
        };

        context.GalleryImages.Add(galleryImage);
        await context.SaveChangesAsync(cancellationToken);

        // return Gallery Image ID to be used in frontend to download the export
        return new ExportCustomerListQueryResponse
        {
            Success = true,
            Message = "Participant list exported successfully.",
            GalleryImageId = galleryImage.GalleryImageId.ToString()
        };
    }

    private (string? CompanyName, List<string>? PreferredSports) ExtractFieldsFromCustomerData(string? customerDataJson)
    {
        if (string.IsNullOrWhiteSpace(customerDataJson))
        {
            return (null, null);
        }

        JObject jObject;
        try
        {
            jObject = JObject.Parse(customerDataJson);
        }
        catch (JsonReaderException)
        {
            return (null, null);
        }

        var companyName = jObject["CompanyName"]?.ToString();
        var preferredSports = jObject["PreferredSports"]?.ToObject<List<string>>();

        return (companyName, preferredSports);
    }
}
