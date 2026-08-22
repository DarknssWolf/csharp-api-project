using CSharpAPIProject.Application.VendorLedger.Commands.CreditLedger;
using CSharpAPIProject.Application.VendorLedger.Commands.DebitLedger;
using CSharpAPIProject.Application.VendorLedger.Models;
using CSharpAPIProject.Application.VendorLedger.Queries.GetPaginatedLedgerList;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints;

public class VendorLedger : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()

            .MapGet(getVendorLedgerList, "List")

            .MapPut(CreditVendorLedger, "Credit")
            .MapPut(DebitVendorLedger, "Debit")
        ;
    }

    #region Get Methods

    public async Task<PaginatedList<VendorLedgerDto>> getVendorLedgerList(ISender sender, int VendorId, int PageNumber, int PageSize)
    {
        return await sender.Send(new GetPaginatedLedgerListQuery
        {
            VendorId = VendorId,
            PageNumber = PageNumber,
            PageSize = PageSize
        });
    }

    #endregion Get Methods

    #region Put Methods

    public async Task<IResult> CreditVendorLedger(ISender sender, int VendorId, string Description, decimal CreditAmount)
    {
        await sender.Send(new CreditLedgerCommand
        {
            VendorId = VendorId,
            Description = Description,
            CreditAmount = CreditAmount,
            Reference = "Manual Credit"
        });
        return Results.Ok();
    }

    public async Task<IResult> DebitVendorLedger(ISender sender, int VendorId, string Description, decimal DebitAmount)
    {
        await sender.Send(new DebitLedgerCommand
        {
            VendorId = VendorId,
            Description = Description,
            DebitAmount = DebitAmount,
            Reference = "Manual Debit"
        });
        return Results.Ok();
    }

    #endregion Put Methods

}