namespace CSharpAPIProject.Application.Models;

public class AccessTokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresIn { get; set; }
} 