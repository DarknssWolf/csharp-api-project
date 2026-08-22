namespace CSharpAPIProject.Application.VoteSets.Models;
public class VoteSetDataDto
{
	public int VoteSetId { get; set; }
	public Guid? UploadId { get; set; }
	public Guid? GalleryImageId { get; set; } = Guid.Empty;
	public string FileDescription { get; set; } = "";
	public string? FileType { get; set; }
	public string? FileName { get; set; }
}
