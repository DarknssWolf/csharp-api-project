namespace CSharpAPIProject.Application.Interfaces.Services;

public interface IAuthHelper
{
    Task<bool> LoggedIn_IsClientAdmin();
    Task<bool> LoggedIn_IsTICUser();

}
