using CSharpAPIProject.Application.Customers.Commands.CreateCustomer;
using CSharpAPIProject.Application.Customers.Commands.DeleteCustomer;
using CSharpAPIProject.Application.Customers.Commands.UpsertCustomer;
using CSharpAPIProject.Application.Customers.Models;
using CSharpAPIProject.Application.Customers.Queries.ExportCustomerList;
using CSharpAPIProject.Application.Customers.Queries.GetCustomer;
using CSharpAPIProject.Application.Customers.Queries.GetPaginatedCustomerList;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class Customers : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.RequireAuthorization()
				.MapGet(GetCustomer, "")
				.MapGet(GetPaginatedCustomerList, "List")
				.MapGet(ExportCustomerList, "ExportCustomerList")

				.MapPost(UpsertCustomer, "Upsert")
				.MapPost(CreateCustomer, "Create")

				.MapDelete(DeleteCustomer, "{customerId}")
			;
		}

		#region Get Methods 

		public async Task<CustomerDto> GetCustomer(ISender sender, string EmailAddress)
		{
            var t = await sender.Send(new GetCustomerQuery() { CustomerEmail = EmailAddress });

            return t;
		}
		 
		public async Task<PaginatedList<CustomerDto>> GetPaginatedCustomerList(ISender sender, int pageNumber = 1, int pageSize = 10, string queryFilter = "")
		{
			return await sender.Send(new GetPaginatedCustomerListQuery()
				{
					PageNumber = pageNumber,
					PageSize = pageSize,
					QueryFilter = queryFilter
				}
			);
		}

		public async Task<ExportCustomerListQueryResponse> ExportCustomerList(ISender sender)
		{
			var exportQuery = new ExportCustomerListQuery();
			var result = await sender.Send(exportQuery);

			return result;
		}

		#endregion Get Methods

		#region Post Methods

		public async Task<IResult> CreateCustomer(ISender sender, CreateCustomerCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}

		public async Task<IResult> DeleteCustomer(ISender sender, int customerId)
		{
			await sender.Send(new DeleteCustomerCommand(customerId));
			return Results.Ok();
		}

		public async Task<IResult> UpsertCustomer(ISender sender, UpsertCustomerCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}

		#endregion Post Methods

	}
}
