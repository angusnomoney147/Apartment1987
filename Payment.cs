using System;

namespace ApartmentManagementSystem
{
    public class Payment
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }  // Changed from PaymentMethod to Method to match your code
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}