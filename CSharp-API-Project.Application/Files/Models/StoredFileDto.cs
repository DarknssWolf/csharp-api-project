namespace CSharpAPIProject.Application.Files.Models;

public class StoredFileDto
{
	public Guid StoredFileId { get; set; }
	public string FileName { get; set; }
	public string ContentType { get; set; }
	public byte[] Content { get; set; }
}
