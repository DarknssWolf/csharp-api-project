using CSharpAPIProject.Application.Suppliers.Commands.CreateSupplier;
using CSharpAPIProject.Application.Suppliers.Commands.DeleteSupplier;
using CSharpAPIProject.Application.Suppliers.Commands.UpsertSupplier;
using CSharpAPIProject.Application.Suppliers.Commands.ResetSupplierApiKey;
using CSharpAPIProject.Application.Suppliers.Commands.DeleteSupplierApiUser;
using CSharpAPIProject.Application.Suppliers.Models;
using CSharpAPIProject.Application.Suppliers.Queries.GetPaginatedSupplierList;
using CSharpAPIProject.Application.Suppliers.Queries.GetSupplier;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints;

public class Suppliers : EndpointGroupBase
{
	public override void Map(WebApplication app)
	{
		app.MapGroup(this)
			.RequireAuthorization()
			
			.MapPost(CreateSupplier, "Create")
			.MapPost(UpdateSupplier, "Upsert")
			.MapPost(ResetSupplierApiKey, "ResetApiKey")
			.MapDelete(DeleteSupplierApiUser, "DeleteApiUser")

			.MapDelete(DeleteSupplier, "Delete")

			.MapGet(GetSupplierList, "List")
			.MapGet(GetSupplier, "");
		;
	}

	#region Post Methods
	public async Task<IResult> CreateSupplier(ISender sender, CreateSupplierCommand request)
	{
		await sender.Send(request);
		return Results.Ok();
	}

	public async Task<IResult> UpdateSupplier(ISender sender, UpsertSupplierCommand request)
	{
		await sender.Send(request);
		return Results.Ok();
	}

	public async Task<ResetSupplierApiKeyCommandResponse> ResetSupplierApiKey(ISender sender, ResetSupplierApiKeyCommand command)
	{
		return await sender.Send(command);
	}

	#endregion Post Methods

	#region Delete Methods

	public async Task<IResult> DeleteSupplier(ISender sender, int supplierId)
	{
		await sender.Send(new DeleteSupplierCommand() { SupplierId = supplierId });
		return Results.Ok();
	}

	public async Task<IResult> DeleteSupplierApiUser(ISender sender, int supplierId)
	{
		var result = await sender.Send(new DeleteSupplierApiUserCommand { SupplierId = supplierId });
		return result ? Results.Ok() : Results.NotFound();
	}

	#endregion Delete methods

	#region Get Methods

	public async Task<PaginatedList<SupplierDto>> GetSupplierList(ISender sender, int pageNumber = 1, int pageSize = 10, string searchFilter = "")
	{
		GetPaginatedSupplierListQuery query = new() { PageNumber = pageNumber, PageSize = pageSize, Search = searchFilter };
		return await sender.Send(query);
	}

	public async Task<SupplierDto> GetSupplier(ISender sender, string? supplierCode = "", int? supplierId = 0)
	{
		return await sender.Send(new GetSupplierQuery() { SupplierCode = supplierCode ?? "", SupplierId = supplierId ?? 0 });
	}

	#endregion Get Methods
}
