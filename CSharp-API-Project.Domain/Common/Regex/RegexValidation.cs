namespace CSharpAPIProject.Domain.Common.Regex;

public static class RegexValidation
{
	/*
	* ^: Asserts the position at the start of the string.
	* (https?://)?: Matches the protocol part of the URL (e.g., http:// or https://). 
		The 's?' makes the 's' optional, so it matches both http and https.
		The entire group is optional due to the '?' at the end.
	* ([\da-z\.-]+): Matches the domain name part of the URL.
		[\da-z\.-]: Matches any digit (\d), lowercase letter (a-z), dot (.), or hyphen (-).
		+: Ensures that the preceding character class appears one or more times.
	* (:\d+)?: Matches an optional port number.
		:\d+: Matches a colon followed by one or more digits.
		?: Ensures that the preceding group is optional.
	* \.([a-z\.]{2,6}): Matches the top-level domain (TLD) part of the URL.
		\.: Matches a literal dot.
		[a-z\.]{2,6}: Matches between 2 and 6 lowercase letters or dots.
	* ([/\w\.-]*)*: Matches the path part of the URL.
		[/\w\.-]: Matches any forward slash (/), word character (\w), dot (.), or hyphen (-).
		*: Ensures that the preceding character class appears zero or more times.
	* /?: Matches an optional trailing slash at the end of the URL.
	* $: Asserts the position at the end of the string.
	*/
	public static string URLValidationPattern = @"^(https?://)?([\da-z\.-]+)(:\d+)?\.([a-z\.]{2,6})([/\w\.-]*)*/?$";
}
