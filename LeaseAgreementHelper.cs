using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ApartmentManagementSystem
{
    public static class LeaseAgreementHelper
    {
        public static string GenerateLeaseAgreement(Lease lease, Tenant tenant, Unit unit, Property property)
        {
            var template = GetTemplate();

            template = template.Replace("[START_DATE]", lease.StartDate.ToString("MMMM dd, yyyy"));
            template = template.Replace("[END_DATE]", lease.EndDate.ToString("MMMM dd, yyyy"));
            template = template.Replace("[MONTHLY_RENT]", lease.MonthlyRent.ToString("F2"));
            template = template.Replace("[SECURITY_DEPOSIT]", lease.SecurityDeposit.ToString("F2"));
            template = template.Replace("[DUE_DATE]", "1st");

            template = template.Replace("[LANDLORD_NAME]", "Property Management Company");
            template = template.Replace("[LANDLORD_ADDRESS]", "123 Management St, City, State");

            template = template.Replace("[TENANT_NAME]", tenant.FullName);
            template = template.Replace("[TENANT_ADDRESS]", tenant.Address ?? "N/A");

            template = template.Replace("[Unit_Number]", unit.UnitNumber);
            template = template.Replace("[PROPERTY_ADDRESS]", property.Address ?? property.Name);

            return template;
        }

        private static string GetTemplate()
        {
            return @"LEASE AGREEMENT

This Lease Agreement is made and entered into on [START_DATE], by and between:

LANDLORD:
[LANDLORD_NAME]
[LANDLORD_ADDRESS]

TENANT:
[TENANT_NAME]
[TENANT_ADDRESS]

PROPERTY:
[Unit_Number] at [PROPERTY_ADDRESS]

LEASE TERMS:
- Lease Start Date: [START_DATE]
- Lease End Date: [END_DATE]
- Monthly Rent: $[MONTHLY_RENT]
- Security Deposit: $[SECURITY_DEPOSIT]
- Payment Due Date: [DUE_DATE] of each month

GENERAL CONDITIONS:
1. The Tenant agrees to pay the monthly rent on or before the due date.
2. The Tenant agrees to maintain the property in good condition.
3. The Tenant agrees to comply with all applicable laws and regulations.
4. The Landlord agrees to provide essential services (water, electricity, etc.).
5. This lease is subject to the terms and conditions set forth in the attached rules and regulations.

TERM:
This lease shall commence on [START_DATE] and shall continue on a month-to-month basis until terminated by either party upon thirty (30) days written notice.

SIGNATURES:

LANDLORD:
___________________________          Date: ________________
[LANDLORD_NAME]

TENANT:
___________________________          Date: ________________
[TENANT_NAME]

WITNESS:
___________________________          Date: ________________";
        }

        public static void SaveLeaseAgreement(string agreement, string fileName)
        {
            try
            {
                File.WriteAllText(fileName, agreement);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving lease agreement: {ex.Message}");
            }
        }

        public static void ExportLeaseAgreementToPdf(string agreement, string fileName)
        {
            try
            {
                using var writer = new PdfWriter(fileName);
                using var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("LEASE AGREEMENT")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20));

                document.Add(new Paragraph(""));

                var lines = agreement.Split('\n');
                foreach (var line in lines)
                {
                    document.Add(new Paragraph(line));
                }

                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting lease agreement to PDF: {ex.Message}");
            }
        }
    }
}