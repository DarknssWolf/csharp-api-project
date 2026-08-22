using CSharpAPIProject.Application.Interfaces.Entities;
using CSharpAPIProject.Application.UserRoles.Exceptions;
using CSharpAPIProject.Application.UserRoles.Helpers.PromotionHelpers;
using CSharpAPIProject.Domain.Common;

namespace CSharpAPIProject.Application.UserRoles.Queries.GetPromotionPaths;

public class GetPromotionPathQuery : IRequest<List<string?>>;

public class GetPromotionPathQueryHandler(
	IUser user, 
	IIdentityService identityService
) : IRequestHandler<GetPromotionPathQuery, List<string?>>
{
    public async Task<List<string?>> Handle(GetPromotionPathQuery request, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new UserNotLoggedInException();

        var roles = await identityService.GetRolesAsync(user);

        // If you don't have any roles then you can't promote anyone.  
        if (roles == null)
            return new List<string?>();

		return PromotionHelper.GetValidRolesToGive(roles.ToList());
	}
}