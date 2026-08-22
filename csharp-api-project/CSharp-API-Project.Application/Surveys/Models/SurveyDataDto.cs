namespace CSharpAPIProject.Application.Surveys.Models;

public class SurveyDataDto
{
	public Guid? UploadId { get; set; }
	public Guid? GalleryImageId { get; set; } = Guid.Empty;
	public string FileDescription { get; set; } = "";
	public string? FileType { get; set; }
	public string? FileName { get; set; }
}
