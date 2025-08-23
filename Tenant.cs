using System;

namespace ApartmentManagementSystem
{
    public class Tenant
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UnitNumber { get; set; }
        public string PropertyName { get; set; }
        public bool IsActive { get; set; }
        public decimal? RentAmount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }

        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
    }
}