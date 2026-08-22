namespace CSharpAPIProject.Application.Surveys.Models;

public class SurveyDto
{
	public int SurveyId { get; set; }
	public string Title { get; set; }
	public string Code { get; set; }
	public DateTime StartOn { get; set; }
	public DateTime EndOn { get; set; }
	public bool CanEarnPoints { get; set; }
	public bool AllowMultipleTries { get; set; }
	public int MultipleTriesLimit { get; set; }
	public int Points { get; set; }
	public string? SurveyData { get; set; }
	public Guid? VisibilityConfigurationId { get; set; }
	public IEnumerable<SurveyQuestionDto> Questions { get; set; }
	public int TotalQuestions { get; set; }
}
