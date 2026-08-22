using CSharpAPIProject.Application.Interfaces.Entities;
using System.Security.Claims;

namespace CSharp_API_Project.API.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
	public string? Id => httpContextAccessor.HttpContext?.User?
		.FindFirstValue(ClaimTypes.NameIdentifier);

	public int SupplierId
	{
		get
		{
			string? id = httpContextAccessor.HttpContext?.User?.FindFirstValue("SupplierId");

			if (int.TryParse(id, out int supplierId))
				return supplierId;

			return 0;
		}
	}

	public int VendorId
	{
		get
		{
			string? id = httpContextAccessor.HttpContext?.User?.FindFirstValue("VendorId");

			if (int.TryParse(id, out int vendorId))
				return vendorId;

			return 0;
		}
	}

	public int CustomerId
	{
		get
		{
			string? id = httpContextAccessor.HttpContext?.User?.FindFirstValue("CustomerId");

			if (int.TryParse(id, out int vendorId))
				return vendorId;

			return 0;
		}
	}

    public Guid UserId
    {
        get{
            string? idAsString = httpContextAccessor.HttpContext?.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);
            Guid userId;

            if (Guid.TryParse(idAsString, out userId))
                return userId;
            else return Guid.Empty;
        }
    }

	public bool IsImpersonated =>
		httpContextAccessor.HttpContext?.User?.FindFirstValue("IsImpersonated") == "y";

	public string? ImpersonatedBy =>
		httpContextAccessor.HttpContext?.User?.FindFirstValue("ImpersonatingUserId");
}
