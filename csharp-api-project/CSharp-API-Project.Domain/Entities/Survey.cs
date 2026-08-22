using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.Entities;

public class Survey : BaseAuditableEntity
{
	// PK
	public int SurveyId { get; set; }
	public int VendorId { get; set; }

	// Properties
	public string Title { get; set; }
	public string Code { get; set; }
	public DateTime StartOn { get; set; }
	public DateTime EndOn { get; set; }
	public bool CanEarnPoints { get; set; }
	public bool AllowMultipleTries { get; set; }
	public int MultipleTriesLimit { get; set; }
	public int Points { get; set; }
	public string? SurveyData { get; set; } //Used for storing optional data related to the survey in JSON
	public Guid? VisibilityConfigurationId { get; set; }
	public RecordStatus RecordStatus { get; set; }
	public DateTimeOffset? RecordDeletedDate { get; set; }

	// Navigation Properties
	public ICollection<SurveyQuestion> SurveyQuestions { get; set; }
	public Vendor Vendor { get; set; }
}
