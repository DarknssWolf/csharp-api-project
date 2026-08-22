using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Domain.Entities;

public class SurveyResponseSet : BaseAuditableEntity
{
    // PK
    public int SurveyResponseSetId { get; set; }

    // FK
    public int SurveyId { get; set; }
    public int CustomerId { get; set; }
    public int VendorId { get; set; }

    // Properties
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CompletedQuestions { get; set; }
    public int TotalQuestions { get; set; }
    public int LastQuestionIndex { get; set; }
    public ResponseSetStatus ResponseSetStatus { get; set; }

    // Navigation Properties
    public Survey Survey { get; set; }
    public Customer Customer { get; set; }
    public Vendor Vendor { get; set; }
}
