using ApartmentManagementSystem;

public static class PaymentStatusHelper
{
    public static string GetStatusName(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "Pending",
            PaymentStatus.Completed => "Paid",
            PaymentStatus.Overdue => "Overdue",
            PaymentStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }
}