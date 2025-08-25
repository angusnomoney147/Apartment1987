using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class MainWindow : Window
    {
        private readonly PropertyRepository _propertyRepository;
        private readonly UnitRepository _unitRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly LeaseRepository _leaseRepository;
        private readonly PaymentRepository _paymentRepository;
        private readonly MaintenanceRepository _maintenanceRepository;
        private List<Property> _properties = new();

        // Event to notify when data changes
        public static event EventHandler DataChanged;

        public MainWindow()
        {
            InitializeComponent();
            DatabaseManager.InitializeDatabase();
            _propertyRepository = new PropertyRepository();
            _unitRepository = new UnitRepository();
            _tenantRepository = new TenantRepository();
            _leaseRepository = new LeaseRepository();
            _paymentRepository = new PaymentRepository();
            _maintenanceRepository = new MaintenanceRepository();

            // Subscribe to data change events
            SubscribeToDataChanges();

            LoadDashboardData();
        }

        private void SubscribeToDataChanges()
        {
            // Subscribe to the global data changed event
            DataChanged += (sender, e) => LoadDashboardData();
        }

        // Call this method whenever data changes
        public static void NotifyDataChanged()
        {
            DataChanged?.Invoke(null, EventArgs.Empty);
        }

        public void ManualRefresh()
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                LoadStatistics();
                LoadRecentActivity();
                LoadVacantUnits();
                LoadOccupiedUnits();
                LoadTenantActivity();
                LoadMaintenanceStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentActivity()
        {
            try
            {
                var tenants = _tenantRepository.GetAll();
                var properties = _propertyRepository.GetAll();
                var maintenance = _maintenanceRepository.GetAll();
                var payments = _paymentRepository.GetAll();

                var recentActivities = new List<object>();

                // Recent tenants
                var recentTenants = tenants.OrderByDescending(t => t.CreatedDate).Take(5)
                    .Select(t => new
                    {
                        Date = t.CreatedDate,
                        Type = "👥 New Tenant",
                        Description = $"{t.FullName} added"
                    });
                recentActivities.AddRange(recentTenants);

                // Recent properties
                var recentProperties = properties.OrderByDescending(p => p.CreatedDate).Take(5)
                    .Select(p => new
                    {
                        Date = p.CreatedDate,
                        Type = "🏘️ New Property",
                        Description = $"{p.Name} added"
                    });
                recentActivities.AddRange(recentProperties);

                // Recent maintenance
                var recentMaintenance = maintenance.OrderByDescending(m => m.RequestDate).Take(5)
                    .Select(m => new
                    {
                        Date = m.RequestDate,
                        Type = "🔧 Maintenance",
                        Description = $"Request for Unit {GetUnitNumber(m.UnitId)}: {m.Description.Substring(0, Math.Min(30, m.Description.Length))}..."
                    });
                recentActivities.AddRange(recentMaintenance);

                // Recent payments
                var recentPayments = payments.OrderByDescending(p => p.PaymentDate).Take(5)
                    .Select(p => new
                    {
                        Date = p.PaymentDate,
                        Type = "💰 Payment",
                        Description = $"${p.Amount:F2} received for Lease {p.LeaseId}"
                    });
                recentActivities.AddRange(recentPayments);

                var sortedActivities = recentActivities.OrderByDescending(a => a.GetType().GetProperty("Date").GetValue(a)).Take(15).ToList();
                DataGridRecentActivity.ItemsSource = sortedActivities;
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        private string GetUnitNumber(int unitId)
        {
            try
            {
                var unit = _unitRepository.GetById(unitId);
                return unit?.UnitNumber ?? $"Unit {unitId}";
            }
            catch
            {
                return $"Unit {unitId}";
            }
        }

        private void LoadStatistics()
        {
            try
            {
                var properties = _propertyRepository.GetAll();
                var units = _unitRepository.GetAll();
                var leases = _leaseRepository.GetAll();
                var maintenanceRequests = _maintenanceRepository.GetAll();
                var payments = _paymentRepository.GetAll();

                // Use safe conversion with default values
                TxtTotalProperties.Text = properties?.Count.ToString() ?? "0";
                TxtTotalUnits.Text = units?.Count.ToString() ?? "0";

                var occupiedUnits = units?.Count(u => u.Status == UnitStatus.Occupied) ?? 0;
                TxtOccupiedUnits.Text = occupiedUnits.ToString();

                var vacantUnits = units?.Count(u => u.Status == UnitStatus.Vacant) ?? 0;
                TxtVacantUnits.Text = vacantUnits.ToString();

                var pendingMaintenance = maintenanceRequests?.Count(m =>
                    m.Status == MaintenanceStatus.Pending || m.Status == MaintenanceStatus.InProgress) ?? 0;
                TxtPendingMaintenance.Text = pendingMaintenance.ToString();

                var overduePayments = payments?.Count(p => p.Status == PaymentStatus.Overdue) ?? 0;
                TxtOverduePayments.Text = overduePayments.ToString();

                var activeLeases = leases?.Count(l => l.Status == LeaseStatus.Active) ?? 0;
                TxtActiveLeases.Text = activeLeases.ToString();
            }
            catch (Exception ex)
            {
                // Set default values when there's an error
                TxtTotalProperties.Text = "0";
                TxtTotalUnits.Text = "0";
                TxtOccupiedUnits.Text = "0";
                TxtVacantUnits.Text = "0";
                TxtPendingMaintenance.Text = "0";
                TxtOverduePayments.Text = "0";
                TxtActiveLeases.Text = "0";

                // Log the error but don't show message to user to avoid spam
                Console.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        private void LoadVacantUnits()
        {
            try
            {
                // Order properties by name ascending
                var properties = _propertyRepository.GetAll().OrderBy(p => p.Name).ToList();
                var units = _unitRepository.GetAll().Where(u => u.Status == UnitStatus.Vacant).ToList();

                var vacantUnits = units.Select(u => new
                {
                    PropertyName = GetPropertyName(u.PropertyId, properties),
                    UnitNumber = u.UnitNumber ?? "Unknown",
                    UnitType = u.UnitType ?? "N/A",
                    RentAmount = u.RentAmount,
                    Bedrooms = u.Bedrooms
                })
                .OrderBy(x => x.PropertyName) // Order by PropertyName ascending
                .ToList();

                DataGridVacantUnits.ItemsSource = vacantUnits;
            }
            catch (Exception ex)
            {
                DataGridVacantUnits.ItemsSource = new List<object>();
                Console.WriteLine($"Error loading vacant units: {ex.Message}");
            }
        }

        private void LoadOccupiedUnits()
        {
            try
            {
                // Order properties by name ascending
                var properties = _propertyRepository.GetAll().OrderBy(p => p.Name).ToList();
                var units = _unitRepository.GetAll().Where(u => u.Status == UnitStatus.Occupied).ToList();
                var leases = _leaseRepository.GetAll().Where(l => l.Status == LeaseStatus.Active).ToList();
                var tenants = _tenantRepository.GetAll();

                var occupiedUnits = units.Select(u =>
                {
                    var lease = leases.FirstOrDefault(l => l.UnitId == u.Id);
                    var tenant = lease != null ? tenants.FirstOrDefault(t => t.Id == lease.TenantId) : null;

                    return new
                    {
                        PropertyName = GetPropertyName(u.PropertyId, properties),
                        UnitNumber = u.UnitNumber ?? "Unknown",
                        TenantName = tenant?.FullName ?? "Unknown Tenant",
                        LeaseStart = lease?.StartDate ?? DateTime.MinValue,
                        MonthlyRent = lease?.MonthlyRent ?? 0,
                        LeaseEnd = lease?.EndDate ?? DateTime.MinValue
                    };
                })
                .OrderBy(x => x.PropertyName) // Order by PropertyName ascending
                .ToList();

                DataGridOccupiedUnits.ItemsSource = occupiedUnits;
            }
            catch (Exception ex)
            {
                DataGridOccupiedUnits.ItemsSource = new List<object>();
                Console.WriteLine($"Error loading occupied units: {ex.Message}");
            }
        }

        private void LoadTenantActivity()
        {
            try
            {
                // Order properties by name ascending
                var properties = _propertyRepository.GetAll().OrderBy(p => p.Name).ToList();
                var units = _unitRepository.GetAll();
                var leases = _leaseRepository.GetAll().Where(l => l.Status == LeaseStatus.Active).ToList();
                var tenants = _tenantRepository.GetAll().Where(t => t.IsActive).ToList();

                var tenantActivities = leases.Select(l =>
                {
                    var unit = units.FirstOrDefault(u => u.Id == l.UnitId);
                    var tenant = tenants.FirstOrDefault(t => t.Id == l.TenantId);
                    var property = unit != null ? properties.FirstOrDefault(p => p.Id == unit.PropertyId) : null;

                    return new
                    {
                        TenantName = tenant?.FullName ?? "Unknown Tenant",
                        PropertyName = property?.Name ?? "Unknown Property",
                        UnitNumber = unit?.UnitNumber ?? "Unknown Unit",
                        LeaseStart = l.StartDate,
                        MonthlyRent = l.MonthlyRent,
                        LeaseStatus = LeaseStatusHelper.GetStatusName(l.Status)
                    };
                })
                .OrderBy(x => x.PropertyName) // Order by PropertyName ascending
                .ToList();

                DataGridTenantActivity.ItemsSource = tenantActivities;
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadMaintenanceStatus()
        {
            try
            {
                // Order properties by name ascending
                var properties = _propertyRepository.GetAll().OrderBy(p => p.Name).ToList();
                var units = _unitRepository.GetAll();
                var maintenanceRequests = _maintenanceRepository.GetAll()
                    .Where(m => m.Status == MaintenanceStatus.Pending || m.Status == MaintenanceStatus.InProgress)
                    .ToList();

                var maintenanceItems = maintenanceRequests.Select(m =>
                {
                    var unit = units.FirstOrDefault(u => u.Id == m.UnitId);
                    var property = unit != null ? properties.FirstOrDefault(p => p.Id == unit.PropertyId) : null;

                    return new
                    {
                        PropertyName = property?.Name ?? "Unknown Property",
                        UnitNumber = unit?.UnitNumber ?? "Unknown Unit",
                        Description = m.Description,
                        Priority = MaintenancePriorityHelper.GetPriorityName(m.Priority),
                        Status = MaintenanceStatusHelper.GetStatusName(m.Status),
                        RequestDate = m.RequestDate,
                        AssignedTo = m.AssignedTo
                    };
                })
                .OrderBy(x => x.PropertyName) // Order by PropertyName ascending
                .ToList();

                DataGridMaintenanceStatus.ItemsSource = maintenanceItems;
            }
            catch (Exception ex)
            {
            }
        }

        // Make sure GetPropertyName method is correct
        private string GetPropertyName(int propertyId, List<Property> properties)
        {
            try
            {
                var property = properties?.FirstOrDefault(p => p.Id == propertyId);
                return property?.Name ?? "Unknown Property";
            }
            catch
            {
                return "Unknown Property";
            }
        }
        private void btnTenants_Click(object sender, RoutedEventArgs e)
        {
            var tenantWindow = new TenantManagementWindow();
            tenantWindow.Show();
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e)
        {
            var propertyWindow = new PropertyManagementWindow();
            propertyWindow.Show();
        }

        private void btnUnits_Click(object sender, RoutedEventArgs e)
        {
            var unitWindow = new UnitManagementWindow();
            unitWindow.Show();
        }

        private void btnLeases_Click(object sender, RoutedEventArgs e)
        {
            var leaseWindow = new LeaseManagementWindow();
            leaseWindow.Show();
        }

        private void btnPayments_Click(object sender, RoutedEventArgs e)
        {
            var paymentWindow = new PaymentManagementWindow();
            paymentWindow.Show();
        }

        private void btnMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var maintenanceWindow = new MaintenanceManagementWindow();
            maintenanceWindow.Show();
        }

        private void btnPropertyReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var propertyReportWindow = new EnhancedPropertyReportWindow();
                propertyReportWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening property report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTenantReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tenants = _tenantRepository.GetAll();
                var leases = _leaseRepository.GetAll();
                var reportContent = ReportHelper.GenerateTenantReport(tenants, leases);
                var reportWindow = new ReportViewerWindow("Tenant Report", reportContent);
                reportWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating tenant report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnFinancialReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var payments = _paymentRepository.GetAll();
                var leases = _leaseRepository.GetAll();
                var tenants = _tenantRepository.GetAll();
                var reportContent = ReportHelper.GenerateFinancialReport(payments, leases, tenants);
                var financialReportWindow = new EnhancedFinancialReportWindow();
                financialReportWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating financial report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnMaintenanceReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var requests = _maintenanceRepository.GetAll();
                var units = _unitRepository.GetAll();
                var reportContent = ReportHelper.GenerateMaintenanceReport(requests, units);
                var reportWindow = new ReportViewerWindow("Maintenance Report", reportContent);
                reportWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating maintenance report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLeaseExpiryReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var leases = _leaseRepository.GetAll();
                var units = _unitRepository.GetAll();
                var tenants = _tenantRepository.GetAll();
                var reportContent = ReportHelper.GenerateLeaseExpiryReport(leases, units, tenants);
                var reportWindow = new ReportViewerWindow("Lease Expiry Report", reportContent);
                reportWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating lease expiry report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ActivityItem
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}