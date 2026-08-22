using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.UserRoles.Exceptions;

public class UserNotLoggedInException() : BadRequestException("You are not logged in. Please log in to use this feature");
public class InsufficientPermissionsToGiveRoleException(string role) : BadRequestException($"You do not have permission to give the role {role}");
public class InsufficientPermissionsToTakeRoleException(string role) : BadRequestException($"You do not have permission to take the role {role}");
public class UserNotFoundException(Guid userId) : BadRequestException($"The user with the id {userId} could not be found");
public class CantRemoveLastRoleForUserException(): BadRequestException("The last role assigned to a user cannot be removed. To proceed with deleting the user, please remove the user account directly.");