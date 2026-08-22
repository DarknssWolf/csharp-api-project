using System.Text.RegularExpressions;

namespace CSharpAPIProject.Domain.ValueObjects;
public class PlainTextHTML :ValueObject
{
    public string HTML { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
    public PlainTextHTML() { } //default constructor for EF Core *REQUIRED*
    public PlainTextHTML(string html, string plainText)
    {
        HTML = html;
        PlainText = StripHtmlTags(plainText);
    }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return HTML;
        yield return StripHtmlTags(PlainText);
    }

    /// <summary>
    /// simple method to remove any residual HTML tags from the plain text
    /// </summary>
    /// <param name="html"></param>
    /// <returns>plain text as a string</returns>
    private stastring StripHtmlTags(string html)
    {
        return Regex.Replace(html, "<.*?>", string.Empty);
    }

}
