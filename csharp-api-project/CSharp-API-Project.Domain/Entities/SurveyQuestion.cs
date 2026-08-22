using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.Entities;

public class SurveyQuestion : BaseAuditableEntity
{
	// PK
	public int SurveyQuestionId { get; set; }

	//Unique keys
	public int SurveyId { get; set; }
	public int QuestionId { get; set; }
	public int? ParentQuestionId { get; set; }

	// Properties
	public string Prompt { get; set; }
	public QuestionType Type { get; set; }
	public string ExpectedAnswers { get; set; } //comma delimited list of correct answers
	public bool AllowMultipleResponses { get; set; }
	public string Options { get; set; } //comma delimited list of available options
	public string? ParentCondition { get; set; } //question is only to be shown if parent answer = parent condition
	public int OrderNumber { get; set; }
	public bool IsRequired { get; set; }
	public string? SurveyQuestionData { get; set; }//Used for storing optional data related to the question in JSON

	// Navigation Properties
	public Survey Survey { get; set; }
	public ICollection<SurveyResponse> Responses { get; set; }// Navigation property for related SurveyResponses
}
