using CSharpAPIProject.Application.Common.Exceptions;

namespace CSharpAPIProject.Application.Vendors.Exceptions
{
    public class EmailAlreadyInUseException(): BadRequestException("This email is already in use. Please use a different email.");
	public class InvalidOtpExceptions(): BadRequestException("The provided OTP is invalid");
    public class VendorNotFoundException(int vendorId): BadRequestException($"Vendor not found for {vendorId}");
	public class VendorListEmptyException(): BadRequestException($"There are no vendors to return at this time. Try adding a vendor first.");
	public class SupplierNotFoundException(string supplierCode): BadRequestException($"Supplier not found for code {supplierCode}");
	public class PendingApplicationExistsException(): BadRequestException($"There is a pending application for this vendor to this supplier. Please delete the previous application before submitting a new one");
	public class ApprovedFeedNotFoundException(): BadRequestException($"No feed with the necessary approved pricelist or stocklocation was found");
}
