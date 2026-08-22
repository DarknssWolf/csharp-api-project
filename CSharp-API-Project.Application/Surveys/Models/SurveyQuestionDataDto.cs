namespace CSharpAPIProject.Application.Surveys.Models;
public class SurveyQuestionDataDto
{
	public int QuestionId { get; set; }
	public Guid? StoredFileId { get; set; } = Guid.Empty;
	public string FileDescription { get; set; } = "";
	public string? FileType { get; set; }
	public string? FileName { get; set; }
}
