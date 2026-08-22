using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.Surveys.Exceptions;

public class SurveyNotFoundException(int SurveyId) : BadRequestException($"Survey with id {SurveyId} was not found.");
public class SurveyWithVendorAndCodeNotFoundException(string Surveycode, int VendorId) : BadRequestException($"Survey with code {Surveycode} and VendorId {VendorId} was not found.");
public class SurveyWithVendorAndSurveyIdNotFoundException(int SurveyId, int VendorId) : BadRequestException($"Survey with ID {SurveyId} and VendorId {VendorId} was not found.");
public class SurveyInDeleteStatusException(string SurveyCode) : BadRequestException($"Survey {SurveyCode} is in Delete status.");
public class QuestionWithParentIdNotFoundException(int SurveyId, int SurveyQuestionId, int? ParentQuestionId) : BadRequestException($"ParentQuestionId {ParentQuestionId} does not exist in Survey with ID {SurveyId} (Question ID: {SurveyQuestionId})");
public class QuestionWithConditionButNoOptionsFoundException(int SurveyId, int SurveyQuestionId, int? ParentQuestionId) : BadRequestException($"SurveyQuestionId {SurveyQuestionId} has a condition set but ParentQuestionID {ParentQuestionId} does not have options set for Survey {SurveyId}");
public class QuestionWithConditionButNoParentException(int SurveyId, int SurveyQuestionId, int? ParentQuestionId) : BadRequestException($"SurveyQuestionId {SurveyQuestionId} has a condition set but No ParentQuestionId set for Survey {SurveyId}");
public class QuestionConditionDoesNotMatchParentOptionException(int SurveyId, int SurveyQuestionId, int? ParentQuestionId) : BadRequestException($"SurveyQuestionId {SurveyQuestionId} condition does not match ParentQuestionID {ParentQuestionId} options for Survey {SurveyId}");
public class GalleryAlreadyExistsException(string newGalleryCode) : BadRequestException($"FAILED: A Gallery with the code [{newGalleryCode}] already exists.");
public class VendorNotFoundException() : BadRequestException("A Vendor with the requested tenant Id was not found.");
public class GalleryNotFoundException(int GalleryId) : BadRequestException($"A Gallery with ID {GalleryId} was not found.");
public class GalleryImageNotFoundException() : BadRequestException($"A Gallery image was not found.");
public class OpenTextNotSupportedException(int questionId) : BadRequestException($"Question {questionId} - Cannot create open text questions for Surveys");
public class ExpectedAnswersCannotBeEmptyException(int questionId) : BadRequestException($"Question {questionId} - ExpectedAnswers cannot be null or empty");
public class OptionsCannotBeEmptyException(int questionId) : BadRequestException($"Question {questionId} - Options cannot be null or empty");
public class ExpectedAnswerNotInOptionsExceptions(string expectedAnswer, int questionId) : BadRequestException($"{expectedAnswer} does not exist in question {questionId} options");
