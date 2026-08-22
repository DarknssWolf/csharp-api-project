using CSharpAPIProject.Application.Common.Exceptions;
using CSharpAPIProject.Application.Common.Security;
using System.Reflection;
using Microsoft.Extensions.Logging;


namespace CSharpAPIProject.Application.Common.Behaviors;

public class AuthorizationBehaviour<TRequest, TResponse>(
    IGlobalIdentityService globalIdentityService,
    IUser user,
    IIdentityService identityService,
    IAuthHelper authHelper,
    ILogger<AuthorizationBehaviour<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
	private readonly IUser _user = user;
	private readonly IIdentityService _identityService = identityService;
    private readonly IGlobalIdentityService _globalIdentityService = globalIdentityService;
    private readonly IAuthHelper _authHelper = authHelper;
    private readonly ILogger<AuthorizationBehaviour<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
        _logger.LogInformation("AUTHORIZATION BEHAVIOUR: Handling request of type {RequestType}", typeof(TRequest).Name);
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

		if (authorizeAttributes.Any())
		{
			// Must be authenticated user
			if (_user.Id == null)
			{
				throw new UnauthorizedAccessException();
			}

			// Role-based authorization
			var authorizeAttributesWithRoles = authorizeAttributes.Where(a =>
				!string.IsNullOrWhiteSpace(a.Roles)
			);

			if (authorizeAttributesWithRoles.Any())
			{
				var authorized = false;

				foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
				{
					foreach (var role in roles)
					{
                        bool isInRole = false;
                        if (await _authHelper.LoggedIn_IsPlatformAdmin())
                            isInRole = await _globalIdentityService.IsInRoleAsync(_user.Id, role.Trim());
                        else
                            isInRole = await _identityService.IsInRoleAsync(_user.Id, role.Trim());

                        if (isInRole)
                        {
                            authorized = true;
                            break;
                        }
					}
				}

				// Must be a member of at least one role in roles
				if (!authorized)
				{
					throw new ForbiddenAccessException();
				}
			}

			// Policy-based authorization
			var authorizeAttributesWithPolicies = authorizeAttributes.Where(a =>
				!string.IsNullOrWhiteSpace(a.Policy)
			);
			if (authorizeAttributesWithPolicies.Any())
			{
				var authorized = false;
				foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
				{
					if(await _identityService.AuthorizeAsync(_user.Id, policy))
					{
						authorized = true;
						break;
					}
				}

				if (!authorized)
				{
					throw new ForbiddenAccessException();
				}
			}
		}

		// User is authorized / authorization not required
		var response = await next();

        _logger.LogInformation("AUTHORIZATION BEHAVIOUR: Handled request of type {RequestType}", typeof(TRequest).Name);

        return response;
    }
}
