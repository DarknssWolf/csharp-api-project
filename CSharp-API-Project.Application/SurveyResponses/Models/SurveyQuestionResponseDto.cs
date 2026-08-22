namespace CSharpAPIProject.Application.SurveyResponses.Models
{
	public class SurveyQuestionResponseDto
	{
		public int SurveyQuestionId { get; set; }
		public string Prompt { get; set; }
		public string Response { get; set; }
		public DateTimeOffset LastModifiedOn { get; set; }
	}
}
