namespace CSharpAPIProject.Domain.ValueObjects;

public class Address : ValueObject
{
	public String Street { get; set; }
	public String Street2 { get; set; }
	public String City { get; set; }
	public String Province { get; set; }
	public String Country { get; set; }
	public String PostalCode { get; set; }

	public Address() { }

	public Address(string street, string street2, string city, string province, string country, string postalCode)
	{
		Street = street;
		Street2 = street2;
		City = city;
		Province = province;
		Country = country;
		PostalCode = postalCode;
	}

	protected override IEnumerable<object> GetEqualityComponents()
	{
		// Using a yield return statement to return each element one at a time
		yield return Street;
		yield return Street2;
		yield return City;
		yield return Province;
		yield return Country;
		yield return PostalCode;
	}
}
