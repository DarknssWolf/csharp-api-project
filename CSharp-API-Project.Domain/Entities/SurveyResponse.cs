using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.Entities
{
	public class SurveyResponse : BaseAuditableEntity
	{
		// PK
		public int SurveyResponseId { get; set; }

		// Properties
		public int VendorId { get; set; }
		public int SurveyId { get; set; }
		public int SurveyQuestionId { get; set; }
		public int QuestionId { get; set; }
		public int CustomerId { get; set; }
		public int? SurveyResponseSetId { get; set; }
		public string Response { get; set; }

		// Navigation Properties
		public Survey Survey { get; set; }
		public SurveyQuestion SurveyQuestion { get; set; }
		public SurveyResponseSet SurveyResponseSet { get; set; }
	}
}
