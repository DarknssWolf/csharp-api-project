using CSharpAPIProject.Domain.ValueObjects;

namespace CSharpAPIProject.Application.Customers.Models
{
	public class CustomerDto
	{
		public int CustomerId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string IDNumber { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public Address Address { get; set; }
		public string CustomerData { get; set; }
        public string UserGroup { get; set; }
        public string UserSubGroup { get; set; }
        public Guid RegionId { get; set; }

    }
}
