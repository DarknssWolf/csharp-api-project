using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.Users.Exceptions;

public class PasswordChangeFailedExceptions(): BadRequestException("Your Password couldn't be changed. Please try again and ensure you use a strong password.");
public class ChangePasswordEmailMismatchException(): BadRequestException($"Email mismatch exception");
public class ChangePasswordWrongPasswordException(): BadRequestException($"Email mismatch exception");
public class UserDataEntityNotFound(string EntityName): BadRequestException($"Data Entity [{EntityName}] for the requested user was not found.");
public class UserNotFoundException(string Email) : BadRequestException($"User with email [{Email}] was not found.");
public class UserAlreadyExistsException(string Email) : BadRequestException($"User with email [{Email}] already exists.");
public class RoleDoesNotExistException(string Role) : BadRequestException($"Role [{Role}] does not exist or you do not have sufficient permissions to view it.");
public class FailedToAddUserToRoleException(string Role) : BadRequestException($"Failed to add user to role [{Role}].");
public class UserAlreadyInRoleException(string Role) : BadRequestException($"User is already in role [{Role}].");
public class InvalidRefreshTokenException(): BadRequestException("Invalid refresh token.");
public class InvalidRefreshExpirationException(): BadRequestException("Refresh token expiration is invalid.");
public class RefreshTokenExpiredException() : BadRequestException("Refresh token has expired.");
public class InvalidUserIdException(): BadRequestException("Logged in user has invalid user credentials.");
public class UserInDeleteStatusException() : BadRequestException("User account is in deleted status.");
public class VendorNotFoundException(int VendorId) : BadRequestException($"A vendor with VendorId {VendorId} was not found.");
public class UserWithIdNotFoundException(Guid UserId) : BadRequestException($"A user with the id [{UserId}] was not found.");
public class InsufficientPermissionsToManageUserException() : BadRequestException("You do not have permission to manage this user.");
