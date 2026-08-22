using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.UserGroups.Exceptions;
public class UserGroupNotFoundException(Guid groupId) : BadRequestException($"User Group with id {groupId} was not found.");
public class UserSubGroupNotFoundException(Guid subGroupId) : BadRequestException($"User Sub Group with id {subGroupId} was not found.");