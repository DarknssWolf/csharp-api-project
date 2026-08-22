using CSharpAPIProject.Application.Common.Exceptions;
using CSharpAPIProject.Application.Interfaces.Entities;
using Microsoft.AspNetCore.Http;

public class TenantResolver : ITenantResolver
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	protected int cachedTenantId = -2;
    protected int cachedUserTenantId = -2;

	public TenantResolver(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public int ResolveTenantId()
	{
		if (cachedTenantId != -2) return cachedTenantId;

		var context = _httpContextAccessor.HttpContext;
		if (context?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId) == true)
		{
			if (string.IsNullOrEmpty(tenantId))
				return SetTenantId(-1);

			return SetTenantId(int.Parse(tenantId));
		}
		else if (context == null)
			return SetTenantId(0); // Assume that only Program.cs can access the site without a HTTPContext.

		throw new BadRequestException("Unable to identify Tenant. Ensure that the tenant is specified.");
	}

	public int ResolveUserTenantId()
	{
		if (cachedUserTenantId != -2) return cachedUserTenantId;

		var context = _httpContextAccessor.HttpContext;
        if (context?.Request.Headers.TryGetValue("X-User-Tenant-Id", out var userTenantId) == true)
        {
            if (string.IsNullOrEmpty(userTenantId))
                return SetUserTenantId(-1);

            return SetUserTenantId(int.Parse(userTenantId));
        }
        else return ResolveTenantId();
	}

	public int SetTenantId(int tenantId)
	{
		cachedTenantId = tenantId;
		return cachedTenantId;
	}

	public int SetUserTenantId(int tenantId)
	{
        cachedUserTenantId = tenantId;
		return cachedUserTenantId;
	}

	public void ResetTenantId()
	{
		SetTenantId(-2);
	}
}
