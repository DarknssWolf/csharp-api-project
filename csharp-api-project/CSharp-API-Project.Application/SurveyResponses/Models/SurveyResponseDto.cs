namespace CSharpAPIProject.Application.SurveyResponses.Models
{
	public class SurveyResponseDto
	{
		public int VendorId { get; set; }
		public int SurveyId { get; set; }
		public int CustomerId { get; set; }
		public int? SurveyResponseSetId { get; set; }
		public string CustomerName { get; set; }
		public string CustomerEmail { get; set; }
		public ICollection<SurveyQuestionResponseDto> Responses { get; set; }
	}
}
