namespace ApartmentManagementSystem
{
    public static class UnitStatusHelper
    {
        public static string GetStatusName(UnitStatus status)
        {
            return status switch
            {
                UnitStatus.Vacant => "Vacant",
                UnitStatus.Occupied => "Occupied",
                UnitStatus.Maintenance => "Maintenance",
                UnitStatus.Reserved => "Reserved",
                _ => "Unknown"
            };
        }
    }
}