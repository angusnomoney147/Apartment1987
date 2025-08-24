using System;

namespace ApartmentManagementSystem
{
    public class Property
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string ManagerName { get; set; }
        public string ContactInfo { get; set; }
        public int TotalUnits { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}