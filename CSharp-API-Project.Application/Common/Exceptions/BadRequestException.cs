namespace CSharpAPIProject.Application.Common.Exceptions
{
	public class BadRequestException(string info): Exception($"The request was invalid: {info}");
	
}
