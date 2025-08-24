using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class LeaseManagementWindow : Window
    {
        private readonly LeaseRepository _leaseRepository;
        private readonly UnitRepository _unitRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly PropertyRepository _propertyRepository;
        private List<Lease> _allLeases = new();
        private List<Unit> _allUnits = new();
        private List<Tenant> _allTenants = new();
        private List<Property> _allProperties = new();

        public LeaseManagementWindow()
        {
            InitializeComponent();
            _leaseRepository = new LeaseRepository();
            _unitRepository = new UnitRepository();
            _tenantRepository = new TenantRepository();
            _propertyRepository = new PropertyRepository();
            InitializeForm(); // Add this line
            LoadAllData();
        }

        private void InitializeForm()
        {
            // Populate status filter dropdown
            var statuses = new List<object>
            {
                new { Value = -1, Name = "All Statuses" }
            };

            statuses.AddRange(Enum.GetValues(typeof(LeaseStatus)).Cast<LeaseStatus>()
                .Select(s => new { Value = (int)s, Name = LeaseStatusHelper.GetStatusName(s) })
                .ToList());

            CmbStatusFilter.ItemsSource = statuses; // Use CmbStatusFilter instead of CmbStatus
            CmbStatusFilter.DisplayMemberPath = "Name";
            CmbStatusFilter.SelectedValuePath = "Value";
            CmbStatusFilter.SelectedIndex = 0; // Default to "All Statuses"
        }

        private void LoadAllData()
        {
            try
            {
                _allLeases = _leaseRepository.GetAll();
                _allUnits = _unitRepository.GetAll();
                _allTenants = _tenantRepository.GetAll();
                _allProperties = _propertyRepository.GetAll();

                RefreshLeaseGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading leases: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshLeaseGrid()
        {
            var leasesWithInfo = _allLeases.Select(l => new
            {
                l.Id,
                PropertyInfo = GetPropertyInfo(l.UnitId),
                UnitInfo = GetUnitInfo(l.UnitId),
                TenantInfo = GetTenantInfo(l.TenantId),
                l.StartDate,
                l.EndDate,
                l.MonthlyRent,
                l.SecurityDeposit,
                Status = LeaseStatusHelper.GetStatusName(l.Status),
                l.CreatedDate
            }).ToList();

            DataGridLeases.ItemsSource = leasesWithInfo;
        }

        private string GetPropertyInfo(int unitId)
        {
            var unit = _allUnits.FirstOrDefault(u => u.Id == unitId);
            if (unit != null)
            {
                var property = _allProperties.FirstOrDefault(p => p.Id == unit.PropertyId);
                return property?.Name ?? "Unknown Property";
            }
            return "Unknown Property";
        }

        private string GetUnitInfo(int unitId)
        {
            var unit = _allUnits.FirstOrDefault(u => u.Id == unitId);
            return unit != null ? unit.UnitNumber : $"Unit {unitId}";
        }

        private string GetTenantInfo(int tenantId)
        {
            var tenant = _allTenants.FirstOrDefault(t => t.Id == tenantId);
            return tenant != null ? tenant.FullName : $"Tenant {tenantId}";
        }

        private void NotifyParentOfChanges()
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.ManualRefresh();
        }

        private void BtnAddLease_Click(object sender, RoutedEventArgs e)
        {
            var addLeaseWindow = new AddEditLeaseWindow(_allProperties, _allUnits, _allTenants);
            if (addLeaseWindow.ShowDialog() == true)
            {
                LoadAllData();
                NotifyParentOfChanges();
            }
        }

        private void BtnEditLease_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridLeases.SelectedItem != null)
            {
                var selectedItem = DataGridLeases.SelectedItem;
                var leaseIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedLeaseId = leaseIdProperty != null ? (int)leaseIdProperty.GetValue(selectedItem) : 0;

                if (selectedLeaseId > 0)
                {
                    var leaseToEdit = _allLeases.FirstOrDefault(l => l.Id == selectedLeaseId);
                    if (leaseToEdit != null)
                    {
                        var editLeaseWindow = new AddEditLeaseWindow(leaseToEdit, _allProperties, _allUnits, _allTenants);
                        if (editLeaseWindow.ShowDialog() == true)
                        {
                            LoadAllData();
                            NotifyParentOfChanges();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a lease to edit.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteLease_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridLeases.SelectedItem != null)
            {
                var selectedItem = DataGridLeases.SelectedItem;
                var leaseIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedLeaseId = leaseIdProperty != null ? (int)leaseIdProperty.GetValue(selectedItem) : 0;

                if (selectedLeaseId > 0)
                {
                    var leaseToDelete = _allLeases.FirstOrDefault(l => l.Id == selectedLeaseId);
                    if (leaseToDelete != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete the lease for Unit {GetUnitInfo(leaseToDelete.UnitId)}?",
                                                   "Confirm Delete",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _leaseRepository.Delete(leaseToDelete.Id);
                                LoadAllData();
                                NotifyParentOfChanges();
                                MessageBox.Show("Lease deleted successfully!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting lease: {ex.Message}", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a lease to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbStatusFilter.SelectedItem != null)
            {
                var selectedValue = (int)CmbStatusFilter.SelectedValue;
                if (selectedValue == -1)
                {
                    // Show all leases
                    RefreshLeaseGrid();
                }
                else
                {
                    // Filter by specific status
                    var filteredLeases = _allLeases.Where(l => (int)l.Status == selectedValue).ToList();
                    RefreshFilteredLeaseGrid(filteredLeases);
                }
            }
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            CmbStatusFilter.SelectedIndex = 0; // Select "All Statuses"
            RefreshLeaseGrid();
        }

        private void BtnActiveOnly_Click(object sender, RoutedEventArgs e)
        {
            var activeLeases = _allLeases.Where(l => l.Status == LeaseStatus.Active).ToList();
            RefreshFilteredLeaseGrid(activeLeases);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                RefreshLeaseGrid();
            }
            else
            {
                var filteredLeases = _allLeases.Where(l =>
                    GetUnitInfo(l.UnitId).ToLower().Contains(searchText) ||
                    GetTenantInfo(l.TenantId).ToLower().Contains(searchText) ||
                    l.Terms?.ToLower().Contains(searchText) == true).ToList();

                RefreshFilteredLeaseGrid(filteredLeases);
            }
        }

        private void RefreshFilteredLeaseGrid(List<Lease> leases)
        {
            var leasesWithInfo = leases.Select(l => new
            {
                l.Id,
                PropertyInfo = GetPropertyInfo(l.UnitId),
                UnitInfo = GetUnitInfo(l.UnitId),
                TenantInfo = GetTenantInfo(l.TenantId),
                l.StartDate,
                l.EndDate,
                l.MonthlyRent,
                l.SecurityDeposit,
                Status = LeaseStatusHelper.GetStatusName(l.Status),
                l.CreatedDate
            }).ToList();

            DataGridLeases.ItemsSource = leasesWithInfo;
        }

        private void BtnDocuments_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridLeases.SelectedItem != null)
            {
                var selectedItem = DataGridLeases.SelectedItem;
                var leaseIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedLeaseId = leaseIdProperty != null ? (int)leaseIdProperty.GetValue(selectedItem) : 0;

                if (selectedLeaseId > 0)
                {
                    var lease = _allLeases.FirstOrDefault(l => l.Id == selectedLeaseId);
                    var unit = _allUnits.FirstOrDefault(u => u.Id == lease?.UnitId);
                    var tenant = _allTenants.FirstOrDefault(t => t.Id == lease?.TenantId);

                    if (lease != null && unit != null && tenant != null)
                    {
                        var documentsWindow = new LeaseDocumentsWindow(lease, unit, tenant);
                        documentsWindow.Show();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a lease to view documents.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}