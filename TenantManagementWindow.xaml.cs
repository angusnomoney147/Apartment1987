using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class TenantManagementWindow : Window
    {
        private readonly TenantRepository _tenantRepository;
        private readonly LeaseRepository _leaseRepository;
        private List<Tenant> _allTenants = new();
        private List<Lease> _allLeases = new();

        public TenantManagementWindow()
        {
            InitializeComponent();
            _tenantRepository = new TenantRepository();
            _leaseRepository = new LeaseRepository();
            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                _allTenants = _tenantRepository.GetAll();
                _allLeases = _leaseRepository.GetAll();
                RefreshTenantGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading  {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTenantGrid()
        {
            var tenantsWithInfo = _allTenants.Select(t => new
            {
                t.Id,
                t.FullName,
                t.Email,
                t.Phone,
                t.EmergencyContact,
                t.EmergencyPhone,
                t.NationalId,
                t.DateOfBirth,
                t.Address,
                ActiveLeases = _allLeases.Count(l => l.TenantId == t.Id && l.Status == LeaseStatus.Active),
                t.CreatedDate,
                Status = t.IsActive ? "Active" : "Inactive"
            }).ToList();

            DataGridTenants.ItemsSource = tenantsWithInfo;
        }

        private void NotifyParentOfChanges()
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.ManualRefresh();
        }

        private void BtnAddTenant_Click(object sender, RoutedEventArgs e)
        {
            var addTenantWindow = new AddEditTenantWindow();
            if (addTenantWindow.ShowDialog() == true)
            {
                LoadAllData();
                NotifyParentOfChanges();
            }
        }

        private void BtnEditTenant_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTenants.SelectedItem != null)
            {
                var selectedItem = DataGridTenants.SelectedItem;
                var tenantIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedTenantId = tenantIdProperty != null ? (int)tenantIdProperty.GetValue(selectedItem) : 0;

                if (selectedTenantId > 0)
                {
                    var tenantToEdit = _allTenants.FirstOrDefault(t => t.Id == selectedTenantId);
                    if (tenantToEdit != null)
                    {
                        var editTenantWindow = new AddEditTenantWindow(tenantToEdit);
                        if (editTenantWindow.ShowDialog() == true)
                        {
                            LoadAllData();
                            NotifyParentOfChanges();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a tenant to edit.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteTenant_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTenants.SelectedItem != null)
            {
                var selectedItem = DataGridTenants.SelectedItem;
                var tenantIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedTenantId = tenantIdProperty != null ? (int)tenantIdProperty.GetValue(selectedItem) : 0;

                if (selectedTenantId > 0)
                {
                    var tenantToDelete = _allTenants.FirstOrDefault(t => t.Id == selectedTenantId);
                    if (tenantToDelete != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete {tenantToDelete.FullName}?",
                                                   "Confirm Delete",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _tenantRepository.Delete(tenantToDelete.Id);
                                LoadAllData();
                                NotifyParentOfChanges();
                                MessageBox.Show("Tenant deleted successfully!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting tenant: {ex.Message}", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a tenant to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.ToLower(); // Changed from TxtSearch to txtSearch
            if (string.IsNullOrWhiteSpace(searchText))
            {
                RefreshTenantGrid();
            }
            else
            {
                var filteredTenants = _allTenants.Where(t =>
                    t.FullName.ToLower().Contains(searchText) ||
                    (t.Email?.ToLower().Contains(searchText) ?? false) ||
                    (t.Phone?.Contains(searchText) ?? false) ||
                    (t.Address?.ToLower().Contains(searchText) ?? false)).ToList();

                var tenantsWithInfo = filteredTenants.Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.Email,
                    t.Phone,
                    t.EmergencyContact,
                    t.EmergencyPhone,
                    t.NationalId,
                    t.DateOfBirth,
                    t.Address,
                    ActiveLeases = _allLeases.Count(l => l.TenantId == t.Id && l.Status == LeaseStatus.Active),
                    t.CreatedDate,
                    Status = t.IsActive ? "Active" : "Inactive"
                }).ToList();

                DataGridTenants.ItemsSource = tenantsWithInfo; // Changed from DataGridTenants to dataGridTenants
            }
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            RefreshTenantGrid();
        }

        private void BtnActiveOnly_Click(object sender, RoutedEventArgs e)
        {
            var activeTenants = _allTenants.Where(t => t.IsActive).ToList();
            var tenantsWithInfo = activeTenants.Select(t => new
            {
                t.Id,
                t.FullName,
                t.Email,
                t.Phone,
                t.EmergencyContact,
                t.EmergencyPhone,
                t.NationalId,
                t.DateOfBirth,
                t.Address,
                ActiveLeases = _allLeases.Count(l => l.TenantId == t.Id && l.Status == LeaseStatus.Active),
                t.CreatedDate,
                Status = t.IsActive ? "Active" : "Inactive"
            }).ToList();

            DataGridTenants.ItemsSource = tenantsWithInfo;
        }
    }
}