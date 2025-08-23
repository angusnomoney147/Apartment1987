using System;

namespace ApartmentManagementSystem
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int TenantId { get; set; }
        public string Description { get; set; } = string.Empty;
        public MaintenancePriority Priority { get; set; }
        public MaintenanceStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? Cost { get; set; }
        public string AssignedTo { get; set; } = string.Empty;
    }

    public enum MaintenancePriority
    {
        Low,
        Medium,
        High,
        Emergency
    }

    public enum MaintenanceStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}