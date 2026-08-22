namespace CSharpAPIProject.Domain.Common;

public abstract class Roles
{
    public const string Administrator = nameof(Administrator); // platform admin (TenantId 0): creates surveys/vote sets, manages users
    public const string Vendor = nameof(Vendor);        // tenant admin: manages their tenant's participants and content
    public const string Customer = nameof(Customer);      // tenant participant: answers surveys, votes, creates vote items
}
