namespace CSharpAPIProject.Domain.ValueObjects
{
	public class EmailAddress : ValueObject
	{
		public string Name { get; set; }
		public string Address { get; set; }

		public EmailAddress() { }

		public EmailAddress(string name, string address)
		{
			Name = name;
			Address = address;
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			// Using a yield return statement to return each element one at a time
			yield return Name;
			yield return Address;
		}
	}
}
