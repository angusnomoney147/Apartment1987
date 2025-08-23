using System;

namespace ApartmentManagementSystem
{
    public class Payment
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod Method { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public enum PaymentMethod
    {
        Cash,
        BankTransfer,
        CreditCard,
        Check
    }

    public enum PaymentStatus
    {
        Paid,
        Pending,
        Overdue,
        Cancelled
    }
}