using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApartmentManagementSystem
{
    public static class ReportHelper
    {
        public static string GeneratePropertyReport(List<Property> properties, List<Unit> units)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PropertyParams REPORT");
            sb.AppendLine("==================");
            sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            foreach (var property in properties)
            {
                var propertyUnits = units.Where(u => u.PropertyId == property.Id).ToList();
                var occupiedUnits = propertyUnits.Count(u => u.Status == UnitStatus.Occupied);

                sb.AppendLine($"PropertyParams: {property.Name}");
                sb.AppendLine($"  Address: {property.Address}");
                sb.AppendLine($"  City: {property.City}");
                sb.AppendLine($"  Total Units: {property.TotalUnits}");
                sb.AppendLine($"  Occupied Units: {occupiedUnits}");
                sb.AppendLine($"  Vacant Units: {propertyUnits.Count - occupiedUnits}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GenerateTenantReport(List<Tenant> tenants, List<Lease> leases)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TENANT REPORT");
            sb.AppendLine("=============");
            sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            foreach (var tenant in tenants.Where(t => t.IsActive))
            {
                var tenantLeases = leases.Where(l => l.TenantId == tenant.Id).ToList();
                var activeLeases = tenantLeases.Count(l => l.Status == LeaseStatus.Active);

                sb.AppendLine($"Tenant: {tenant.FullName}");
                sb.AppendLine($"  Email: {tenant.Email}");
                sb.AppendLine($"  Phone: {tenant.Phone}");
                sb.AppendLine($"  Active Leases: {activeLeases}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GenerateFinancialReport(List<Payment> payments, List<Lease> leases, List<Tenant> tenants)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FINANCIAL REPORT");
            sb.AppendLine("===============");
            sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            var totalPayments = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount); // Changed from Paid to Completed
            var thisMonthPayments = payments.Where(p => p.Status == PaymentStatus.Completed && // Changed from Paid to Completed
                                                       p.PaymentDate.Month == DateTime.Now.Month &&
                                                       p.PaymentDate.Year == DateTime.Now.Year)
                                           .Sum(p => p.Amount);

            sb.AppendLine($"Total Payments Received: ${totalPayments:F2}");
            sb.AppendLine($"This Month Payments: ${thisMonthPayments:F2}");
            sb.AppendLine();

            sb.AppendLine("Recent Payments:");
            var recentPayments = payments.Where(p => p.Status == PaymentStatus.Completed) // Changed from Paid to Completed
                                        .OrderByDescending(p => p.PaymentDate)
                                        .Take(10);

            foreach (var payment in recentPayments)
            {
                var lease = leases.FirstOrDefault(l => l.Id == payment.LeaseId);
                var tenant = lease != null ? tenants.FirstOrDefault(t => t.Id == lease.TenantId) : null;
                var tenantName = tenant?.FullName ?? "Unknown Tenant";

                sb.AppendLine($"  {payment.PaymentDate:yyyy-MM-dd}: ${payment.Amount:F2} - {tenantName}");
            }

            return sb.ToString();
        }

        public static string GenerateMaintenanceReport(List<MaintenanceRequest> requests, List<Unit> units)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MAINTENANCE REPORT");
            sb.AppendLine("=================");
            sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            var pendingRequests = requests.Count(r => r.Status == MaintenanceStatus.Pending);
            var inProgressRequests = requests.Count(r => r.Status == MaintenanceStatus.InProgress);
            var completedRequests = requests.Count(r => r.Status == MaintenanceStatus.Completed);

            sb.AppendLine($"Pending Requests: {pendingRequests}");
            sb.AppendLine($"In Progress: {inProgressRequests}");
            sb.AppendLine($"Completed: {completedRequests}");
            sb.AppendLine();

            sb.AppendLine("Pending Maintenance Requests:");
            var pendingList = requests.Where(r => r.Status == MaintenanceStatus.Pending)
                                     .OrderByDescending(r => r.Priority)
                                     .ThenBy(r => r.RequestDate);

            foreach (var request in pendingList)
            {
                var unit = units.FirstOrDefault(u => u.Id == request.UnitId);
                var unitNumber = unit?.UnitNumber ?? "Unknown";
                var priority = MaintenancePriorityHelper.GetPriorityName(request.Priority);

                sb.AppendLine($"  Unit {unitNumber} - {priority}: {request.Description.Substring(0, Math.Min(30, request.Description.Length))}...");
            }

            return sb.ToString();
        }

        public static string GenerateLeaseExpiryReport(List<Lease> leases, List<Unit> units, List<Tenant> tenants)
        {
            var sb = new StringBuilder();
            sb.AppendLine("LEASE EXPIRY REPORT");
            sb.AppendLine("==================");
            sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            var next30Days = DateTime.Now.AddDays(30);
            var expiringLeases = leases.Where(l => l.Status == LeaseStatus.Active &&
                                                  l.EndDate <= next30Days &&
                                                  l.EndDate >= DateTime.Now)
                                      .OrderBy(l => l.EndDate);

            if (!expiringLeases.Any())
            {
                sb.AppendLine("No leases expiring in the next 30 days.");
            }
            else
            {
                sb.AppendLine("Leases expiring in the next 30 days:");
                sb.AppendLine();

                foreach (var lease in expiringLeases)
                {
                    var unit = units.FirstOrDefault(u => u.Id == lease.UnitId);
                    var tenant = tenants.FirstOrDefault(t => t.Id == lease.TenantId);
                    var unitNumber = unit?.UnitNumber ?? "Unknown Unit";
                    var tenantName = tenant?.FullName ?? "Unknown Tenant";

                    sb.AppendLine($"  Unit {unitNumber} - {tenantName}");
                    sb.AppendLine($"    End Date: {lease.EndDate:yyyy-MM-dd}");
                    sb.AppendLine($"    Days Remaining: {(lease.EndDate - DateTime.Now).Days}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}