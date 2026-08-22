namespace CSharpAPIProject.Application.Interfaces.Services;

public interface IAuthHelper
{
    Task<bool> LoggedIn_IsVendorAdmin();
    Task<bool> LoggedIn_IsPlatformAdmin();

}
