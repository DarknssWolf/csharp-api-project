namespace CSharpAPIProject.Domain.Exceptions
{
	public class UnsupportedCurrencyException(string code) : Exception($"{code} is not a supported currency");
}
