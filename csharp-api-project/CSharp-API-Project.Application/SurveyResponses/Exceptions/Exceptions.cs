using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.SurveyResponses.Exceptions;

public class SurveyResponseNotFoundException(int VendorId, int SurveyId, int SurveyQuestionId, int CustomerId) :BadRequestException($"Survey Answer not found for [VendorId: {VendorId}|SurveyId: {SurveyId}|SurveyQuestionId: {SurveyQuestionId}|CustomerId: {CustomerId}] combination.");
public class ResponseNotValidException(int SurveyQuestionId) :BadRequestException($"Survey Answer is not valid for QuestionId {SurveyQuestionId}");
public class UserNotAllowedException() : BadRequestException("You do not have the permissions for this request");
public class NoRetriesAllowedException() : BadRequestException("You have already answered this Survey and cannot change your answers");
