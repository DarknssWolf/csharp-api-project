namespace CSharpAPIProject.Application.SurveyResponseSets.Exceptions;

public class SurveyResponseSetNotFoundException : Exception
{
	public SurveyResponseSetNotFoundException(int surveyResponseSetId)
		: base($"SurveyResponseSet with ID {surveyResponseSetId} was not found.") { }
}

public class SurveyResponseSetWithCustomerAndSurveyNotFoundException : Exception
{
	public SurveyResponseSetWithCustomerAndSurveyNotFoundException(int customerId, int surveyId)
		: base($"SurveyResponseSet with Customer ID {customerId} and Survey ID {surveyId} was not found.") { }
}
