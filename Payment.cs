using ApartmentManagementSystem;

public class Payment
{
    public int Id { get; set; }
    public int LeaseId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; } // Make sure this is PaymentMethod enum
    public string ReferenceNumber { get; set; }
    public PaymentStatus Status { get; set; } // Make sure this is PaymentStatus enum
    public string Notes { get; set; }
    public DateTime CreatedDate { get; set; }

    // Calculated properties
    public decimal? DueAmount { get; set; }
    public DateTime? DueDate { get; set; }
}