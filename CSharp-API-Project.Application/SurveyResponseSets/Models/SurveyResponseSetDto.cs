using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Application.SurveyResponseSets.Models;

public class SurveyResponseSetDto
{
	public int SurveyResponseSetId { get; set; }
	public int SurveyId { get; set; }
	public int CustomerId { get; set; }
	public int VendorId { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public int CompletedQuestions { get; set; }
	public int TotalQuestions { get; set; }
	public int LastQuestionIndex { get; set; }
	public ResponseSetStatus ResponseSetStatus { get; set; }
}
