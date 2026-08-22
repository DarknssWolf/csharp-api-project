namespace CSharpAPIProject.Application.Interfaces.Entities;

public interface ITenantResolver
{
	public int ResolveTenantId();
    public int ResolveUserTenantId();

    public int SetTenantId(int tenantId);
    public int SetUserTenantId(int tenantId);
	public void ResetTenantId();
} 