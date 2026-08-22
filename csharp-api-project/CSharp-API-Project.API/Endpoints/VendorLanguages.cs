using CSharpAPIProject.Application.VendorLanguages.Commands.CreateVendorLanguage;
using CSharpAPIProject.Application.VendorLanguages.Commands.DeleteVendorLanguage;
using CSharpAPIProject.Application.VendorLanguages.Commands.UpdateVendorLanguage;
using CSharpAPIProject.Application.VendorLanguages.Models;
using CSharpAPIProject.Application.VendorLanguages.Queries.GetPaginatedVendorLanguageList;
using CSharpAPIProject.Application.VendorLanguages.Queries.GetVendorLanguage;
using CSharpAPIProject.Application.VendorLanguages.Queries.GetVendorLanguagesByVendor;
using CSharpAPIProject.Application.Models;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints;

public class VendorLanguages : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()

            .MapGet(GetVendorLanguage, "{vendorLanguageId}")
            .MapGet(GetPaginatedVendorLanguageList, "List")
            .MapGet(GetVendorLanguagesByVendor, "ByVendor/{vendorId}")

            .MapPost(CreateVendorLanguage, "Create")
            .MapPost(UpdateVendorLanguage, "Update")

            .MapDelete(DeleteVendorLanguage, "{vendorLanguageId}")
        ;
    }

    #region Get Methods

    public async Task<VendorLanguageDto> GetVendorLanguage(ISender sender, Guid vendorLanguageId)
    {
        return await sender.Send(new GetVendorLanguageQuery { VendorLanguageId = vendorLanguageId });
    }

    public async Task<PaginatedList<VendorLanguageDto>> GetPaginatedVendorLanguageList(
        ISender sender, 
        int pageNumber = 1, 
        int pageSize = 10)
    {
        return await sender.Send(new GetPaginatedVendorLanguageListQuery 
        { 
            PageNumber = pageNumber, 
            PageSize = pageSize
        });
    }

    public async Task<List<VendorLanguageDto>> GetVendorLanguagesByVendor(ISender sender, int vendorId)
    {
        return await sender.Send(new GetVendorLanguagesByVendorQuery { VendorId = vendorId });
    }

    #endregion Get Methods

    #region Post Methods

    public async Task<Guid> CreateVendorLanguage(ISender sender, CreateVendorLanguageCommand command)
    {
        return await sender.Send(command);
    }

    public async Task<IResult> UpdateVendorLanguage(ISender sender, UpdateVendorLanguageCommand command)
    {
        await sender.Send(command);
        return Results.Ok();
    }

    #endregion Post Methods

    #region Delete Methods

    public async Task<IResult> DeleteVendorLanguage(ISender sender, Guid vendorLanguageId)
    {
        await sender.Send(new DeleteVendorLanguageCommand { VendorLanguageId = vendorLanguageId });
        return Results.Ok();
    }

    #endregion Delete Methods
}