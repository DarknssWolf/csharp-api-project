using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.Customers.Exceptions;

public class CustomerWithEmailAlreadyExistsException(string email) : BadRequestException($"Customer with email {email} already exists.");
public class CustomerWithIDNumberAlreadyExistsException(string idNumber) : BadRequestException($"Customer with ID number {idNumber} already exists.");
partial class CustomerNotFoundException(int CustomerId) : BadRequestException($"Customer with Id {CustomerId} does not exist"); // TODO: Change to public class. Donno what this will break lol. Good luck
partial class CustomerWithEmailNotFoundException(string CustomerEmail, int VendorId) : BadRequestException($"Customer {CustomerEmail} does not exist on Vendor {VendorId}."); // TODO: Change to public class. Donno what this will break lol. Good luck
public class VendorNotFoundException(int VendorId) : BadRequestException($"Vendor with Id {VendorId} does not exist");
public class VendorMisMatchException(int VendorId) : BadRequestException($"Vendor with Id {VendorId} is not assigned to this user.");
public class CustomerEmailAndIdNumberMismatchException(string Email, string IdNumber) : BadRequestException($"Another customer with email {Email} or ID number {IdNumber} already exists.");
public class UnauthorizedAccessWithEmailException(string Email) : BadRequestException($"{Email} does not belong to you. Unauthorized Access!");
public class CustomerUserNotFoundException(string Email) : BadRequestException($"A User profile associated with the customer {Email} was not found.");
public class CantDeleteOtherVendorCustomerException() : BadRequestException($"You cannot delete a customer that does not belong to your vendor.");
public class CustomerClaimsNotFoundException(int CustomerId) : BadRequestException($"Could not get customer {CustomerId} claim information");