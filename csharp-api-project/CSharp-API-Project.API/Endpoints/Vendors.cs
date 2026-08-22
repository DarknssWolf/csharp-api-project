using CSharpAPIProject.Application.Settings.Commands.SetVendorSetting;
using CSharpAPIProject.Application.Vendors.Commands.DeleteVendor;
using CSharpAPIProject.Application.Vendors.Commands.RegisterVendor;
using CSharpAPIProject.Application.Vendors.Commands.ResetVendorApiKey;
using CSharpAPIProject.Application.Vendors.Commands.UpdateVendor;
using CSharpAPIProject.Application.Vendors.Commands.DeleteVendorApiUser;
using CSharpAPIProject.Application.Vendors.Models;
using CSharpAPIProject.Application.Vendors.Queries.GetMyVendorStoreData;
using CSharpAPIProject.Application.Vendors.Queries.GetPaginatedVendorList;
using CSharpAPIProject.Application.Vendors.Queries.GetVendorQuery;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Api.Infrastructure;
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

				.MapPost(UpdateVendorSetting, "UpdateSetting")
                .MapPost(UpdateVendor, "Update")

                .MapPut(ResetVendorApiKey, "ResetApiKey")
                .MapDelete(DeleteVendorApiUser, "DeleteApiUser")
		
				.MapGet(GetVendor, "")
				.MapGet(GetVendors, "GetAllVendors")

                .MapGet(GetMyVendorStore, "GetMyVendorStore")

                .MapDelete(DeleteVendor, "{VendorId}");

            ;
		}

		#region Post Methods

		public async Task<IResult> RegisterVendor(ISender sender, RegisterVendorCommand request)
		{
			await sender.Send(request);
			return Results.Ok();
		}

		public async Task<ResetVendorApiKeyCommandResponse> ResetVendorApiKey(ISender sender, ResetVendorApiKeyCommand command)
		{
			return await sender.Send(command);
		}

		public async Task<SetVendorSettingCommandResponse> UpdateVendorSetting(ISender sender, SetVendorSettingCommand request )
		{
			return await sender.Send(request);
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

        public async Task<IResult> DeleteVendorApiUser(ISender sender, int vendorId)
        {
            var result = await sender.Send(new DeleteVendorApiUserCommand { VendorId = vendorId });
            return result ? Results.Ok() : Results.NotFound();
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

		public async Task<VendorStoreDataDto> GetMyVendorStore(ISender sender, int? VendorId)
		{
			var result = await sender.Send(new GetMyVendorStoreDataQuery() { VendorId = VendorId ?? -1});
			return result;
		}

		#endregion Get Methods

	}
}
