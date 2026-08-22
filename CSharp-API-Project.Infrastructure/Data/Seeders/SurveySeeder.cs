using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Domain.Entities;
using CSharpAPIProject.Domain.Enums;

namespace CSharpAPIProject.Infrastructure.Data.Seeders;

public class SurveySeeder(IApplicationDbContext context)
{

    /// <summary>
    /// Seeds default survey data
    /// </summary>
    public async Task SeedDefaultSurveyDataAsync()
    {
        await SeedDefaultSurveyAsync();
        await SeedSurveyResponsesAsync();
    }

    /// <summary>
    /// Seeds the default survey
    /// </summary>
    private async Task SeedDefaultSurveyAsync()
    {
        var vendor = context.Vendors.FirstOrDefault(v => v.Email == "vendor@localhost.com");
        if (vendor == null) return;

        var survey = new Survey
        {
            VendorId = vendor.VendorId,
            Title = "Sample Customer Satisfaction Survey",
            Code = "SURVEY001",
            StartOn = DateTime.UtcNow.AddDays(-30),
            EndOn = DateTime.UtcNow.AddDays(30),
            CanEarnPoints = true,
            AllowMultipleTries = false,
            RecordStatus = RecordStatus.Active,
            CreatedOn = DateTimeOffset.UtcNow
        };

        await context.Surveys.AddAsync(survey);
        await context.SaveChangesAsync(CancellationToken.None);

        // Create survey questions
        var questions = new List<SurveyQuestion>
        {
            new SurveyQuestion
            {
                SurveyId = survey.SurveyId,
                QuestionId = 1,
                Prompt = "How satisfied are you with our service?",
                Type = QuestionType.OpenText,
                //DefaultResponse = "",
                ExpectedAnswers = "",
                Options = "",
                AllowMultipleResponses = false,
                IsRequired = true,
                OrderNumber = 1,
                CreatedOn = DateTimeOffset.UtcNow
            },
            new SurveyQuestion
            {
                SurveyId = survey.SurveyId,
                QuestionId = 2,
                Prompt = "What aspects of our service could be improved?",
                Type = QuestionType.OpenText,
                //DefaultResponse = "",
                ExpectedAnswers = "",
                Options = "",
                AllowMultipleResponses = false,
                IsRequired = false,
                OrderNumber = 2,
                CreatedOn = DateTimeOffset.UtcNow
            },
            new SurveyQuestion
            {
                SurveyId = survey.SurveyId,
                QuestionId = 3,
                Prompt = "Would you recommend our service to others?",
                Type = QuestionType.MultiSelect,
                //DefaultResponse = "",
                ExpectedAnswers = "",
                Options = "Yes,No,Maybe",
                AllowMultipleResponses = false,
                IsRequired = true,
                OrderNumber = 3,
                CreatedOn = DateTimeOffset.UtcNow
            }
        };

        await context.SurveyQuestions.AddRangeAsync(questions);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Seeds survey responses
    /// </summary>
    private async Task SeedSurveyResponsesAsync()
    {
        var vendor = context.Vendors.FirstOrDefault(v => v.Email == "vendor@localhost.com");
        if (vendor == null) return;

        var survey = context.Surveys.FirstOrDefault(s => s.VendorId == vendor.VendorId);
        if (survey == null) return;

        var customer = context.Customers.FirstOrDefault(c => c.VendorId == vendor.VendorId);
        if (customer == null) return;

        var questions = context.SurveyQuestions.Where(sq => sq.SurveyId == survey.SurveyId).ToList();
        if (!questions.Any()) return;

        // Create survey responses for each question
        var surveyResponses = new List<SurveyResponse>
        {
            new SurveyResponse
            {
                VendorId = vendor.VendorId,
                SurveyId = survey.SurveyId,
                SurveyQuestionId = questions.First(q => q.OrderNumber == 1).SurveyQuestionId,
                QuestionId = questions.First(q => q.OrderNumber == 1).QuestionId,
                CustomerId = customer.CustomerId,
                Response = "4",
                CreatedOn = DateTimeOffset.UtcNow
            },
            new SurveyResponse
            {
                VendorId = vendor.VendorId,
                SurveyId = survey.SurveyId,
                SurveyQuestionId = questions.First(q => q.OrderNumber == 2).SurveyQuestionId,
                QuestionId = questions.First(q => q.OrderNumber == 2).QuestionId,
                CustomerId = customer.CustomerId,
                Response = "The response time could be faster, but overall the service is good.",
                CreatedOn = DateTimeOffset.UtcNow
            },
            new SurveyResponse
            {
                VendorId = vendor.VendorId,
                SurveyId = survey.SurveyId,
                SurveyQuestionId = questions.First(q => q.OrderNumber == 3).SurveyQuestionId,
                QuestionId = questions.First(q => q.OrderNumber == 3).QuestionId,
                CustomerId = customer.CustomerId,
                Response = "Yes",
                CreatedOn = DateTimeOffset.UtcNow
            }
        };

        await context.SurveyResponses.AddRangeAsync(surveyResponses);
        await context.SaveChangesAsync(CancellationToken.None);
    }
} 