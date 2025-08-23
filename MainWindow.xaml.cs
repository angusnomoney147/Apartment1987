using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
        private System.Windows.Threading.DispatcherTimer _autoRefreshTimer;

        public MainWindow()
        {
            InitializeComponent();
            TenantDatabaseInitializer.InitializeDatabase();
            MaintenanceDatabaseInitializer.InitializeDatabase();
            DatabaseHelper.InitializeDatabase();
            _propertyRepository = new PropertyRepository();
            _unitRepository = new UnitRepository();
            _tenantRepository = new TenantRepository();
            _leaseRepository = new LeaseRepository();
            _paymentRepository = new PaymentRepository();
            _maintenanceRepository = new MaintenanceRepository();
            InitializeAutoRefresh();
            LoadDashboardData();
        }

        private void InitializeAutoRefresh()
        {
            _autoRefreshTimer = new System.Windows.Threading.DispatcherTimer();
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(30); // Refresh every 30 seconds
            _autoRefreshTimer.Tick += (sender, e) => LoadDashboardData();
            _autoRefreshTimer.Start();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _autoRefreshTimer?.Stop();
            base.OnClosing(e);
        }

        public void ManualRefresh()
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Load statistics
                LoadStatistics();

                // Load all data for dashboard tabs
                var properties = _propertyRepository.GetAll();
                var units = _unitRepository.GetAll();
                var tenants = _tenantRepository.GetAll();
                var leases = _leaseRepository.GetAll();
                var payments = _paymentRepository.GetAll();
                var maintenance = _maintenanceRepository.GetAll();

                // Load recent activity
                LoadRecentActivity(tenants, properties, maintenance, payments);

                // Load tab data
                LoadVacantUnits();
                LoadOccupiedUnits();
                LoadTenantActivity();
                LoadMaintenanceStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard  {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentActivity(List<Tenant> tenants, List<Property> properties, List<MaintenanceRequest> maintenance, List<Payment> payments)
        {
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
                    Description = $"Request for Unit {GetUnitNumber(m.UnitId, _unitRepository.GetAll())}: {m.Description.Substring(0, Math.Min(30, m.Description.Length))}..."
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

        private string GetUnitNumber(int unitId, List<Unit> units)
        {
            var unit = units.FirstOrDefault(u => u.Id == unitId);
            return unit?.UnitNumber ?? $"Unit {unitId}";
        }

        private void LoadStatistics()
        {
            var properties = _propertyRepository.GetAll();
            var units = _unitRepository.GetAll();
            var leases = _leaseRepository.GetAll();
            var maintenanceRequests = _maintenanceRepository.GetAll();
            var payments = _paymentRepository.GetAll();

            TxtTotalProperties.Text = properties.Count.ToString();
            TxtTotalUnits.Text = units.Count.ToString();

            var occupiedUnits = units.Count(u => u.Status == UnitStatus.Occupied);
            TxtOccupiedUnits.Text = occupiedUnits.ToString();

            var pendingMaintenance = maintenanceRequests.Count(m => m.Status == MaintenanceStatus.Pending || m.Status == MaintenanceStatus.InProgress);
            TxtPendingMaintenance.Text = pendingMaintenance.ToString();

            var overduePayments = payments.Count(p => p.Status == PaymentStatus.Overdue);
            TxtOverduePayments.Text = overduePayments.ToString();
        }

        private void LoadVacantUnits()
        {
            try
            {
                var properties = _propertyRepository.GetAll();
                var units = _unitRepository.GetAll().Where(u => u.Status == UnitStatus.Vacant).ToList();

                var vacantUnits = units.Select(u => new
                {
                    PropertyName = GetPropertyName(u.PropertyId, properties),
                    u.UnitNumber,
                    u.UnitType,
                    u.RentAmount,
                    u.Bedrooms
                }).ToList();

                DataGridVacantUnits.ItemsSource = vacantUnits;
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        private void LoadOccupiedUnits()
        {
            try
            {
                var properties = _propertyRepository.GetAll();
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
                        u.UnitNumber,
                        TenantName = tenant?.FullName ?? "Unknown Tenant",
                        LeaseStart = lease?.StartDate ?? DateTime.MinValue,
                        MonthlyRent = lease?.MonthlyRent ?? 0,
                        LeaseEnd = lease?.EndDate ?? DateTime.MinValue
                    };
                }).ToList();

                DataGridOccupiedUnits.ItemsSource = occupiedUnits;
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        private void LoadTenantActivity()
        {
            try
            {
                var properties = _propertyRepository.GetAll();
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
                }).ToList();

                DataGridTenantActivity.ItemsSource = tenantActivities;
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        private void LoadMaintenanceStatus()
        {
            try
            {
                var properties = _propertyRepository.GetAll();
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
                }).ToList();

                DataGridMaintenanceStatus.ItemsSource = maintenanceItems;
            }
            catch (Exception ex)
            {
                // Silently fail
            }
        }

        private string GetPropertyName(int propertyId, List<Property> properties)
        {
            var property = properties.FirstOrDefault(p => p.Id == propertyId);
            return property?.Name ?? "Unknown Property";
        }

        // Management buttons
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

        // Report buttons
        private void btnPropertyReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var properties = _propertyRepository.GetAll();
                var units = _unitRepository.GetAll();
                var reportContent = ReportHelper.GeneratePropertyReport(properties, units);
                var reportWindow = new ReportViewerWindow("PropertyParams Report", reportContent);
                reportWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating property report: {ex.Message}", "Error",
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
                var reportWindow = new ReportViewerWindow("Financial Report", reportContent);
                reportWindow.Show();
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