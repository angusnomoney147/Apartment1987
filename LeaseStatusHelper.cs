namespace ApartmentManagementSystem
{
    public static class LeaseStatusHelper
    {
        public static string GetStatusName(LeaseStatus status)
        {
            return status switch
            {
                LeaseStatus.Active => "Active",
                LeaseStatus.Expired => "Expired",
                LeaseStatus.Terminated => "Terminated",
                LeaseStatus.Pending => "Pending",
                _ => "Unknown"
            };
        }
    }
}