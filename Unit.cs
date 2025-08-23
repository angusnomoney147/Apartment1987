using System;

namespace ApartmentManagementSystem
{
    public class Unit
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public decimal Size { get; set; }
        public decimal RentAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public UnitStatus Status { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
    }

    public enum UnitStatus
    {
        Vacant,
        Occupied,
        Maintenance,
        Reserved
    }
}