namespace CSharpAPIProject.Application.VoteItems.Models;
public class VoteItemDataDto
{
	public int VoteItemId { get; set; }
	public Guid? StoredFileId { get; set; } = Guid.Empty;
	public string FileDescription { get; set; } = "";
	public string? FileType { get; set; }
	public string? FileName { get; set; }
}
