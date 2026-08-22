using CSharpAPIProject.Application.Common.Interfaces;
using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponses.Exceptions;
using CSharpAPIProject.Application.SurveyResponses.Models;
using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Application.SurveyResponses.Queries.GetSurveyResponse
{
	[Authorize(Policy = Policies.CanView_SurveyResponse)]
	public class GetSurveyResponseQuery : IRequest<SurveyResponseDto>
	{
		public int SurveyId { get; set; }
		public int SurveyQuestionId { get; set; }
		public int CustomerId { get; set; }
	}

	public class GetSurveyResponseQueryValidator : AbstractValidator<GetSurveyResponseQuery>
	{
		public GetSurveyResponseQueryValidator()
		{
			RuleFor(x => x.SurveyId).GreaterThan(0);
			RuleFor(x => x.SurveyQuestionId).GreaterThan(0);
			RuleFor(x => x.CustomerId).GreaterThan(0);
		}
	}

	public class GetSurveyResponseQueryHandler(IApplicationDbContext context, IMapper mapper, ITenantResolver tenantResolver) : IRequestHandler<GetSurveyResponseQuery, SurveyResponseDto>
	{
		public async Task<SurveyResponseDto> Handle(GetSurveyResponseQuery request, CancellationToken cancellationToken)
		{
			var vendorId = tenantResolver.ResolveTenantId();

			var surveyResponseDto = await context.SurveyResponses
				.Where(sr => sr.SurveyId == request.SurveyId
					&& sr.VendorId == vendorId
					&& sr.CustomerId == request.CustomerId)
				.Join(
					context.Customers,
					sr => sr.CustomerId,
					c => c.CustomerId,
					(sr, c) => new { sr, c }  // Join SurveyResponses with Customers and get both
				)
				.Join(
					context.SurveyQuestions,  // Now join SurveyQuestions table
					sc => sc.sr.SurveyQuestionId,  // Match SurveyQuestionId from SurveyResponses
					sq => sq.SurveyQuestionId,  // Match SurveyQuestionId in SurveyQuestions table
					(sc, sq) => new SurveyQuestionResponseDto
					{
						SurveyQuestionId = sc.sr.SurveyQuestionId,
						Response = sc.sr.Response,
						Prompt = sq.Prompt  // Assuming there's a 'Prompt' column in SurveyQuestions
					}
				)
				.ToListAsync(cancellationToken);

			// Construct the final DTO manually
			var surveyResponse = new SurveyResponseDto
			{
				VendorId = vendorId,
				SurveyId = request.SurveyId,
				CustomerId = request.CustomerId,
				CustomerName = await context.Customers
					.Where(c => c.CustomerId == request.CustomerId)
					.Select(c => c.FirstName + " " + c.LastName)
					.FirstOrDefaultAsync(cancellationToken),
				Responses = surveyResponseDto
			};



			if (surveyResponse == null)
				throw new SurveyResponseNotFoundException(vendorId, request.SurveyId, request.SurveyQuestionId, request.CustomerId);

			return surveyResponse;
		}
	}
}
