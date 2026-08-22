using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.Surveys.Models;

public class SurveyQuestionDto
{
	public int? SurveyQuestionId { get; set; } //Optional as it is set on insert, but could be passed in for updates
	public int SurveyId { get; set; }
	public int QuestionId { get; set; }
	public int? ParentQuestionId { get; set; }
	public string Prompt { get; set; }
	public QuestionType Type { get; set; }
	public string ExpectedAnswers { get; set; }
	public bool AllowMultipleResponses { get; set; }
	public string Options { get; set; }
	public string? ParentCondition { get; set; } //question is only to be shown if parent answer = parent condition
	public int OrderNumber { get; set; }
	public bool IsRequired { get; set; }
	public string? SurveyQuestionData { get; set; }

	//nested child questions
	public IEnumerable<SurveyQuestionDto>? ChildQuestions { get; set; }
}
