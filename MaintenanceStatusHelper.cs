namespace ApartmentManagementSystem
{
    public static class MaintenanceStatusHelper
    {
        public static string GetStatusName(MaintenanceStatus status)
        {
            return status switch
            {
                MaintenanceStatus.Pending => "Pending",
                MaintenanceStatus.InProgress => "In Progress",
                MaintenanceStatus.Completed => "Completed",
                MaintenanceStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}