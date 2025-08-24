using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class PaymentManagementWindow : Window
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly LeaseRepository _leaseRepository;
        private readonly UnitRepository _unitRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly PropertyRepository _propertyRepository;
        private List<Payment> _allPayments = new();
        private List<Lease> _allLeases = new();
        private List<Unit> _allUnits = new();
        private List<Tenant> _allTenants = new();
        private List<Property> _allProperties = new();

        public PaymentManagementWindow()
        {
            InitializeComponent();
            _paymentRepository = new PaymentRepository();
            _leaseRepository = new LeaseRepository();
            _unitRepository = new UnitRepository();
            _tenantRepository = new TenantRepository();
            _propertyRepository = new PropertyRepository();
            InitializeFilters();
            LoadAllData();
        }

        private void InitializeFilters()
        {
            // Populate status filter dropdown
            var statuses = Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>()
                .Select(s => new { Value = (int)s, Name = PaymentStatusHelper.GetStatusName(s) })
                .ToList();

            CmbStatusFilter.ItemsSource = statuses;
            CmbStatusFilter.DisplayMemberPath = "Name";
            CmbStatusFilter.SelectedValuePath = "Value";

            // Populate method filter dropdown
            var methods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                .Select(m => new { Value = (int)m, Name = PaymentMethodHelper.GetMethodName(m) })
                .ToList();

            CmbMethodFilter.ItemsSource = methods;
            CmbMethodFilter.DisplayMemberPath = "Name";
            CmbMethodFilter.SelectedValuePath = "Value";
        }

        private void LoadAllData()
        {
            try
            {
                _allPayments = _paymentRepository.GetAll();
                _allLeases = _leaseRepository.GetAll();
                _allUnits = _unitRepository.GetAll();
                _allTenants = _tenantRepository.GetAll();
                _allProperties = _propertyRepository.GetAll();

                RefreshPaymentGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading  {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPaymentGrid()
        {
            var paymentsWithInfo = _allPayments.Select(p => new
            {
                p.Id,
                LeaseInfo = GetLeaseInfo(p.LeaseId),
                p.Amount,
                p.PaymentDate,
                Method = PaymentMethodHelper.GetMethodName(p.Method),
                p.ReferenceNumber,
                Status = PaymentStatusHelper.GetStatusName(p.Status),
                p.Notes
            }).ToList();

            DataGridPayments.ItemsSource = paymentsWithInfo;
        }

        private string GetLeaseInfo(int leaseId)
        {
            var lease = _allLeases.FirstOrDefault(l => l.Id == leaseId);
            if (lease != null)
            {
                var unit = _allUnits.FirstOrDefault(u => u.Id == lease.UnitId);
                var tenant = _allTenants.FirstOrDefault(t => t.Id == lease.TenantId);
                var property = unit != null ? _allProperties.FirstOrDefault(p => p.Id == unit.PropertyId) : null;

                var unitNumber = unit?.UnitNumber ?? "Unknown Unit";
                var tenantName = tenant?.FullName ?? "Unknown Tenant";
                var propertyName = property?.Name ?? "Unknown Property";

                return $"{propertyName}, {unitNumber}, {tenantName}";
            }
            return $"Lease {leaseId}";
        }

        private void NotifyParentOfChanges()
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.ManualRefresh();
        }

        private void BtnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            var addPaymentWindow = new AddEditPaymentWindow(_allLeases, _allUnits, _allTenants, _allProperties);
            if (addPaymentWindow.ShowDialog() == true)
            {
                LoadAllData(); // Refresh the list
                NotifyParentOfChanges(); // Add this line
            }
        }

        private void BtnEditPayment_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridPayments.SelectedItem != null)
            {
                // Get the actual payment ID from the selected row
                var selectedItem = DataGridPayments.SelectedItem;
                var paymentIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedPaymentId = paymentIdProperty != null ? (int)paymentIdProperty.GetValue(selectedItem) : 0;

                if (selectedPaymentId > 0)
                {
                    var paymentToEdit = _allPayments.FirstOrDefault(p => p.Id == selectedPaymentId);
                    if (paymentToEdit != null)
                    {
                        var editPaymentWindow = new AddEditPaymentWindow(paymentToEdit, _allLeases, _allUnits, _allTenants, _allProperties);
                        if (editPaymentWindow.ShowDialog() == true)
                        {
                            LoadAllData(); // Refresh the list
                            NotifyParentOfChanges(); // Add this line
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a payment to edit.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridPayments.SelectedItem != null)
            {
                // Get the actual payment ID from the selected row
                var selectedItem = DataGridPayments.SelectedItem;
                var paymentIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedPaymentId = paymentIdProperty != null ? (int)paymentIdProperty.GetValue(selectedItem) : 0;

                if (selectedPaymentId > 0)
                {
                    var paymentToDelete = _allPayments.FirstOrDefault(p => p.Id == selectedPaymentId);
                    if (paymentToDelete != null)
                    {
                        var leaseInfo = GetLeaseInfo(paymentToDelete.LeaseId);

                        var result = MessageBox.Show($"Are you sure you want to delete the payment for {leaseInfo}?",
                                                   "Confirm Delete",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _paymentRepository.Delete(paymentToDelete.Id);
                                LoadAllData(); // Refresh the list
                                NotifyParentOfChanges(); // Add this line
                                MessageBox.Show("Payment deleted successfully!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting payment: {ex.Message}", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a payment to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbStatusFilter.SelectedItem != null)
            {
                var selectedStatus = (int)CmbStatusFilter.SelectedValue;
                var filteredPayments = _allPayments.Where(p => (int)p.Status == selectedStatus).ToList();
                RefreshFilteredPaymentGrid(filteredPayments);
            }
        }

        private void CmbMethodFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMethodFilter.SelectedItem != null)
            {
                var selectedMethod = (int)CmbMethodFilter.SelectedValue;
                var filteredPayments = _allPayments.Where(p => (int)p.Method == selectedMethod).ToList();
                RefreshFilteredPaymentGrid(filteredPayments);
            }
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            CmbStatusFilter.SelectedIndex = -1;
            CmbMethodFilter.SelectedIndex = -1;
            RefreshPaymentGrid();
        }

        private void BtnPaidOnly_Click(object sender, RoutedEventArgs e)
        {
            var paidPayments = _allPayments.Where(p => p.Status == PaymentStatus.Completed).ToList();
            RefreshFilteredPaymentGrid(paidPayments);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                RefreshPaymentGrid();
            }
            else
            {
                var filteredPayments = _allPayments.Where(p =>
                    GetLeaseInfo(p.LeaseId).ToLower().Contains(searchText) ||
                    p.ReferenceNumber?.ToLower().Contains(searchText) == true ||
                    p.Notes?.ToLower().Contains(searchText) == true).ToList();

                RefreshFilteredPaymentGrid(filteredPayments);
            }
        }

        private void RefreshFilteredPaymentGrid(List<Payment> payments)
        {
            var paymentsWithInfo = payments.Select(p => new
            {
                p.Id,
                LeaseInfo = GetLeaseInfo(p.LeaseId),
                p.Amount,
                p.PaymentDate,
                Method = PaymentMethodHelper.GetMethodName(p.Method),
                p.ReferenceNumber,
                Status = PaymentStatusHelper.GetStatusName(p.Status),
                p.Notes
            }).ToList();

            DataGridPayments.ItemsSource = paymentsWithInfo;
        }
    }
}