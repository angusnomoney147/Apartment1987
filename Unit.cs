using System;

namespace ApartmentManagementSystem
{
    public class Unit
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string UnitNumber { get; set; }
        public string UnitType { get; set; }
        public decimal Size { get; set; }
        public decimal RentAmount { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public string Description { get; set; }
        public UnitStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}