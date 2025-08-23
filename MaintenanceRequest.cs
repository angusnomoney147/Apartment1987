using System;

namespace ApartmentManagementSystem
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int TenantId { get; set; }
        public string UnitNumber { get; set; }
        public string PropertyName { get; set; }
        public string TenantName { get; set; }
        public string Description { get; set; }
        public MaintenancePriority Priority { get; set; }
        public MaintenanceStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? Cost { get; set; }
        public string AssignedTo { get; set; }
    }
}