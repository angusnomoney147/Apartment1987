using System;

namespace ApartmentManagementSystem
{
    public class Lease
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string Terms { get; set; } = string.Empty;
        public LeaseStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public enum LeaseStatus
    {
        Active,
        Expired,
        Terminated,
        Pending
    }
}