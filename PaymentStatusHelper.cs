namespace ApartmentManagementSystem
{
    public static class PaymentStatusHelper
    {
        public static string GetStatusName(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Completed => "Paid",
                PaymentStatus.Pending => "Pending",
                PaymentStatus.Overdue => "Overdue",
                PaymentStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}