using CSharpAPIProject.Application.Files.Commands.DeleteFile;
using CSharpAPIProject.Application.Files.Commands.UploadFile;
using CSharpAPIProject.Application.Files.Queries.GetFile;
using CSharpAPIProject.Api.Infrastructure;

namespace CSharpAPIProject.Api.Endpoints
{
	public class Files : EndpointGroupBase
	{
		public override void Map(WebApplication app)
		{
			var group = app.MapGroup(this)
				.RequireAuthorization();

			group
				.MapGet(GetFile, "{storedFileId}")

				.MapDelete(DeleteFile, "{storedFileId}")
			;

			// Form file binding needs antiforgery explicitly disabled since this is a bearer token API
			group.MapPost("Upload", UploadFile)
				.WithName(nameof(UploadFile))
				.DisableAntiforgery();
		}

		#region Get Methods

		public async Task<IResult> GetFile(ISender sender, Guid storedFileId)
		{
			var result = await sender.Send(new GetFileQuery(storedFileId));

			return Results.File(result.Content, result.ContentType, result.FileName);
		}

		#endregion Get Methods

		#region Post Methods

		public async Task<Guid> UploadFile(ISender sender, IFormFile file)
		{
			return await sender.Send(new UploadFileCommand(file));
		}

		public async Task<IResult> DeleteFile(ISender sender, Guid storedFileId)
		{
			await sender.Send(new DeleteFileCommand() { StoredFileId = storedFileId });
			return Results.Ok();
		}

		#endregion Post Methods
	}
}
