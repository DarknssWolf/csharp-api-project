using CSharpAPIProject.Application.Vendors.Commands.DeleteVendor;
using CSharpAPIProject.Application.Vendors.Commands.RegisterVendor;
using CSharpAPIProject.Application.Vendors.Commands.UpdateVendor;
using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Application.Vendors.Queries.GetPaginatedVendorList;
using CSharpAPIProject.Application.Vendors.Queries.GetVendorQuery;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
    public class Vendors : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			app.MapGroup(this)
				.MapPost(RegisterVendor, "Register");

			app.MapGroup(this)
				.RequireAuthorization()

                .MapPost(UpdateVendor, "Update")

				.MapGet(GetVendor, "")
				.MapGet(GetVendors, "GetAllVendors")

                .MapDelete(DeleteVendor, "{VendorId}");

            ;
		}

		#region Post Methods

		public async Task<IResult> RegisterVendor(ISender sender, RegisterVendorCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}

		public async Task<IResult> UpdateVendor(ISender sender, UpdateVendorCommand request )
		{
			await sender.Send(request);
            return Results.Ok();
        }

        #endregion Post Methods

        #region Delete Methods
        public async Task<IResult> DeleteVendor(ISender sender, int VendorId)
        {
            await sender.Send(new DeleteVendorCommand() { VendorId = VendorId });
            return Results.Ok();
        }

        #endregion Delete Methods

        #region Get Methods

        public async Task<VendorDto> GetVendor(ISender sender, int VendorId = -1)
		{
			return await sender.Send(new GetVendorQuery() { VendorId = VendorId});
		}

		public async Task<PaginatedList<VendorDto>> GetVendors(ISender sender, int PageNumber, int PageSize)
		{
			var result = await sender.Send(new GetVendorsQuery() { PageNumber = PageNumber, PageSize = PageSize});
			return result;
		}

		#endregion Get Methods

	}
}
