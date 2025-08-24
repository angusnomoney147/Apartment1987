using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace ApartmentManagementSystem
{
    public partial class EnhancedFinancialReportWindow : Window
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly LeaseRepository _leaseRepository;
        private readonly UnitRepository _unitRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly PropertyRepository _propertyRepository;
        private List<Payment> _allPayments;
        private List<Lease> _allLeases;
        private List<Unit> _allUnits;
        private List<Tenant> _allTenants;
        private List<Property> _allProperties;

        public EnhancedFinancialReportWindow()
        {
            InitializeComponent();
            _paymentRepository = new PaymentRepository();
            _leaseRepository = new LeaseRepository();
            _unitRepository = new UnitRepository();
            _tenantRepository = new TenantRepository();
            _propertyRepository = new PropertyRepository();
            LoadInitialData();
            SetDefaultDates();
        }

        private void LoadInitialData()
        {
            try
            {
                _allPayments = _paymentRepository.GetAll();
                _allLeases = _leaseRepository.GetAll();
                _allUnits = _unitRepository.GetAll();
                _allTenants = _tenantRepository.GetAll();
                _allProperties = _propertyRepository.GetAll();

                // Populate filters
                PopulatePropertyFilter();
                PopulateTenantFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading  {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetDefaultDates()
        {
            DpFromDate.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DpToDate.SelectedDate = DateTime.Now;
        }

        private void PopulatePropertyFilter()
        {
            var properties = new List<object> { new { Id = 0, Name = "All Properties" } };
            properties.AddRange(_allProperties.Select(p => new { p.Id, p.Name }));

            CmbProperties.ItemsSource = properties;
            CmbProperties.DisplayMemberPath = "Name";
            CmbProperties.SelectedValuePath = "Id";
            CmbProperties.SelectedIndex = 0;
        }

        private void PopulateTenantFilter()
        {
            var tenants = new List<object> { new { Id = 0, Name = "All Tenants" } };
            tenants.AddRange(_allTenants.Where(t => t.IsActive).Select(t => new { t.Id, Name = t.FullName }));

            CmbTenants.ItemsSource = tenants;
            CmbTenants.DisplayMemberPath = "Name";
            CmbTenants.SelectedValuePath = "Id";
            CmbTenants.SelectedIndex = 0;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GenerateReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReport()
        {
            var fromDate = DpFromDate.SelectedDate ?? DateTime.Now.AddMonths(-1);
            var toDate = DpToDate.SelectedDate ?? DateTime.Now;
            var propertyId = (int)CmbProperties.SelectedValue;
            var tenantId = (int)CmbTenants.SelectedValue;

            // Filter payments
            var filteredPayments = _allPayments
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .Where(p => p.Status == PaymentStatus.Completed)
                .ToList();

            // Apply property filter
            if (propertyId > 0)
            {
                var propertyLeaseIds = _allLeases
                    .Where(l => _allUnits.Any(u => u.Id == l.UnitId && u.PropertyId == propertyId))
                    .Select(l => l.Id)
                    .ToList();

                filteredPayments = filteredPayments
                    .Where(p => propertyLeaseIds.Contains(p.LeaseId))
                    .ToList();
            }

            // Apply tenant filter
            if (tenantId > 0)
            {
                filteredPayments = filteredPayments
                    .Where(p => _allLeases.Any(l => l.Id == p.LeaseId && l.TenantId == tenantId))
                    .ToList();
            }

            // Generate report data with proper columns
            var reportData = filteredPayments.Select(p =>
            {
                var lease = _allLeases.FirstOrDefault(l => l.Id == p.LeaseId);
                var unit = lease != null ? _allUnits.FirstOrDefault(u => u.Id == lease.UnitId) : null;
                var tenant = lease != null ? _allTenants.FirstOrDefault(t => t.Id == lease.TenantId) : null;
                var property = unit != null ? _allProperties.FirstOrDefault(p => p.Id == unit.PropertyId) : null;

                return new
                {
                    ID = p.Id,
                    PaymentDate = p.PaymentDate,
                    Status = PaymentStatusHelper.GetStatusName(p.Status),
                    PropertyInfo = $"{property?.Name ?? "Unknown Property"}, {unit?.UnitNumber ?? "Unknown Unit"}",
                    RentAmount = p.Amount,
                    TenantName = tenant?.FullName ?? "Unknown Tenant",
                    PhoneNumber = tenant?.Phone ?? "N/A",
                    CreatedDate = p.CreatedDate
                };
            }).ToList();

            DataGridFinancialReport.ItemsSource = reportData;

            // Update summary
            TxtTotalAmount.Text = $"Total Amount: ${reportData.Sum(r => r.RentAmount):F2}";
            TxtRecordCount.Text = $"Records: {reportData.Count}";
        }
    }
}