using System;

namespace ApartmentManagementSystem
{
    public class LeaseDocument
    {
        public int Id { get; set; }
        public int LeaseId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public DateTime UploadDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public enum DocumentType
    {
        LeaseAgreement,
        IDScan,
        Passport,
        DriverLicense,
        Other
    }
}