namespace ApartmentManagementSystem
{
    public static class MaintenancePriorityHelper
    {
        public static string GetPriorityName(MaintenancePriority priority)
        {
            switch (priority)
            {
                case MaintenancePriority.Low:
                    return "Low";
                case MaintenancePriority.Medium:
                    return "Medium";
                case MaintenancePriority.High:
                    return "High";
                case MaintenancePriority.Urgent:
                    return "Urgent";
                default:
                    return priority.ToString();
            }
        }
    }
}