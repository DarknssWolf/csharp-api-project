using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.Common.Security;
using CSharpAPIProject.Application.SurveyResponses.Exceptions;
using CSharpAPIProject.Application.SurveyResponses.Models;
using CSharpAPIProject.Application.Surveys.Models;
using CSharpAPIProject.Domain.Common;
using CSharpAPIProject.Domain.Entities;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace CSharpAPIProject.Application.SurveyResponses.Queries.GetPaginatedSurveyResponseList
{
	[Authorize(Policy = Policies.CanView_SurveyResponse)]
	public record GetPaginatedSurveyResponseListQuery : IRequest<PaginatedList<SurveyResponseDto>>
	{
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int SurveyId { get; set; }
	}

	public class GetPaginatedSurveyResponseListQueryValidator : AbstractValidator<GetPaginatedSurveyResponseListQuery>
	{
		public GetPaginatedSurveyResponseListQueryValidator()
		{
			RuleFor(x => x.PageNumber).GreaterThan(0);
			RuleFor(x => x.PageSize).GreaterThan(0);
			RuleFor(x => x.SurveyId).GreaterThan(0);
		}
	}

	public class GetPaginatedSurveyResponseListQueryHandler(
		IApplicationDbContext context, 
		IMapper mapper, 
		ITenantResolver tenantResolver, 
		IIdentityService identityService,
		 IUser user) : IRequestHandler<GetPaginatedSurveyResponseListQuery, PaginatedList<SurveyResponseDto>>
	{
		public async Task<PaginatedList<SurveyResponseDto>> Handle(GetPaginatedSurveyResponseListQuery request, CancellationToken cancellationToken)
		{
			var vendorId = tenantResolver.ResolveTenantId();

			var surveyQuestions = context.SurveyQuestions
				.Where(s => s.SurveyId == request.SurveyId)
				.Select(q => q.SurveyQuestionId); // Select only the SurveyQuestionId

			// Get all survey responses for the selected SurveyQuestions
			var query = context.SurveyResponses
				.Where(s => surveyQuestions.Contains(s.SurveyQuestionId) && s.VendorId == vendorId)
				.Join(
					context.Customers,
					sr => sr.CustomerId,
					c => c.CustomerId,
					(sr, c) => new { sr, c }  // Temporary anonymous object to hold SurveyResponse and Customer
				)
				.Join(
					context.SurveyQuestions,
					sc => sc.sr.SurveyQuestionId,  // SurveyQuestionId from SurveyResponses
					q => q.SurveyQuestionId,             // QuestionId from Questions table
					(sc, q) => new
					{
						sc.sr.SurveyResponseId,
						sc.sr.SurveyId,
						sc.sr.VendorId,
						sc.sr.SurveyQuestionId,
						sc.sr.QuestionId,
						sc.sr.CustomerId,
						sc.sr.Response,
						sc.sr.LastModifiedOn,
						CustomerName = $"{sc.c.FirstName} {sc.c.LastName}",
						CustomerEmail = sc.c.Email,
						Prompt = q.Prompt  // Assuming the Questions table has a 'Text' column for the question text
					}
				);

			//check logged in user details
			var loggedInUserRoles = await identityService.GetRolesAsync(user);
			var isAdministrator = loggedInUserRoles.Any(role => role == Roles.Administrator);
			var isClientAdmin = loggedInUserRoles.Any(role => role == Roles.Vendor);

			//check permissions
			if (!isAdministrator)
			{
				// If a vendor admin then we need to only filter for their records.
				if (isClientAdmin)
					query = query.Where(s => s.VendorId == vendorId);
				else
				{
					Guid userId = Guid.Parse(user.Id);
					var claims = await identityService.GetClaimsAsync(userId);

					var customerId = claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

					if (string.IsNullOrEmpty(customerId))
						customerId = "0";

					if (customerId == "0")
						throw new UserNotAllowedException();
					else
						query = query.Where(c => c.CustomerId == Convert.ToInt32(customerId));
				}
			}

			//get responses as a list
			var responses = await query.ToListAsync();

			// Group responses by SurveyId & CustomerId using mapper
			var groupedResponses = responses
				.GroupBy(sr => new { sr.CustomerId, sr.VendorId })  // Grouping by Customer & Vendor
				.Select(group => new SurveyResponseDto
				{
					VendorId = group.Key.VendorId,
					SurveyId = group.First().SurveyId,  // Assuming all grouped responses have the same SurveyId
					CustomerId = group.Key.CustomerId,
					CustomerName = group.First().CustomerName, // Get the customer name from any response in the group
					CustomerEmail = group.First().CustomerEmail,
					Responses = group.Select(sr => new SurveyQuestionResponseDto
					{
						SurveyQuestionId = sr.SurveyQuestionId,
						Prompt = sr.Prompt,
						Response = sr.Response,
						LastModifiedOn = sr.LastModifiedOn
					}).ToList()
				})
				.ToList();

			var result = groupedResponses
				.Skip((request.PageNumber - 1) * request.PageSize)
			.Take(request.PageSize)
			.ToList();

			return new PaginatedList<SurveyResponseDto>(result, result.Count, request.PageNumber, request.PageSize); ;
		}
	}
}
