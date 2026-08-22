using CSharpAPIProject.Application.Users.Helpers;
using Microsoft.Extensions.Configuration;
using CSharpAPIProject.Application.Users.Exceptions;

namespace CSharpAPIProject.Application.Users.Commands.LoginUser;

public class LoginUserCommandResponse : AccessTokenResponse
{
}

public class LoginUserCommand : IRequest<LoginUserCommandResponse>
{
    public string Email { get; init; }
    public string Password { get; init; }
}

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty();
        RuleFor(v => v.Password).NotEmpty();
    }
}

public class LoginUserCommandHandler(
    IIdentityService identityService,
    IJwtTokenGeneratorService tokenService,
    IApplicationDbContext context,
    ITenantResolver tenantResolver,
    IConfiguration configuration
) : IRequestHandler<LoginUserCommand, LoginUserCommandResponse>
{
    public async Task<LoginUserCommandResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var vendorId = tenantResolver.ResolveUserTenantId();
        var result = await identityService.LoginUserAsync(request.Email, request.Password, vendorId);

        if (!result)
        {
            throw new UnauthorizedAccessException();
        }

        if(vendorId > 0)
        {
            var vendor = await context.Vendors.Where(x => x.VendorId == vendorId).FirstOrDefaultAsync(cancellationToken);
            if (vendor == null)
                throw new VendorNotFoundException(vendorId);
        }

        var userId = await identityService.GetUserIdAsync(request.Email, vendorId);
        var roles = await identityService.GetRolesAsync(request.Email, vendorId);

        // Check if the user has a status of deleted or suspended. These statuses cannot login.
        var userClaims = await identityService.GetClaimsAsync(Guid.Parse(userId));
        await UserHelper.ValidateUserStatusAsync(userClaims);

        // Resolve tenant ID
        var tenantId = await UserHelper.ResolveTenantIdAsync(request.Email, roles, context, vendorId);

        // Create the token and respond using the helper
        var accessToken =  await UserHelper.CreateAccessTokenResponseAsync(
            userId,
            roles,
            tenantId,
            tokenService,
            configuration,
            identityService);

        return new LoginUserCommandResponse()
        {
            AccessToken = accessToken.AccessToken,
            RefreshToken = accessToken.RefreshToken,
            ExpiresIn = accessToken.ExpiresIn,
        };


    }
}
