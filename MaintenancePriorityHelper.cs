namespace ApartmentManagementSystem
{
    public static class MaintenancePriorityHelper
    {
        public static string GetPriorityName(MaintenancePriority priority)
        {
            return priority switch
            {
                MaintenancePriority.Low => "Low",
                MaintenancePriority.Medium => "Medium",
                MaintenancePriority.High => "High",
                MaintenancePriority.Emergency => "Emergency",
                _ => "Unknown"
            };
        }
    }
}