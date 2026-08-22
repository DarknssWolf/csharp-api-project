using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.Vendors.Exceptions
{
    public class EmailAlreadyInUseException(): BadRequestException("This email is already in use. Please use a different email.");
	public class UserEmailSendFailedException(string context): BadRequestException($"Couldn't send an email to the user: {context}");
	public class InvalidOtpExceptions(): BadRequestException("The provided OTP is invalid");
	public class ApiUserNotFound(int vendorId): BadRequestException($"Api User not found for {vendorId}");
    public class ApiUserCreationFailed(int vendorId) : BadRequestException($"Failed to create API user for vendor {vendorId}. Please try again later or contact support.");
    public class VendorNotFoundException(int vendorId): BadRequestException($"Vendor not found for {vendorId}");
	public class VendorListEmptyException(): BadRequestException($"There are no vendors to return at this time. Try adding a vendor first.");
	public class SupplierNotFoundException(string supplierCode): BadRequestException($"Supplier not found for code {supplierCode}");
	public class PendingApplicationExistsException(): BadRequestException($"There is a pending application for this vendor to this supplier. Please delete the previous application before submitting a new one");
	public class ApprovedFeedNotFoundException(): BadRequestException($"No feed with the necessary approved pricelist or stocklocation was found");
	public class VendorApiUserNotFoundException(int vendorId) : BadRequestException($"Vendor API user not found for vendor {vendorId}");
}
