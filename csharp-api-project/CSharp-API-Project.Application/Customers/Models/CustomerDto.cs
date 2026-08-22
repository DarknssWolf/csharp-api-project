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

        public decimal Balance { get; set; }
        public string PtsAbbr { get; set; } = "";

        // USER PROPERTIES
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? DesignationId { get; set; }
        public string? DesignationName { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public Guid? ResidenceCountryId { get; set; }
        public string? ResidenceCountryName { get; set; }
        public Guid? ServiceCountryId { get; set; }
        public string? ServiceCountryName { get; set; }
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }

    }
}