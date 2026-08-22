namespace CSharpAPIProject.Application.Interfaces.Entities;

public interface IUser
{
	string? Id { get; }
	int VendorId { get; }
    int CustomerId { get; }
    Guid UserId { get; }
} 