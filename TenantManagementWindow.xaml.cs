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
        private List<Tenant> _allTenants = new();

        public TenantManagementWindow()
        {
            InitializeComponent();
            _tenantRepository = new TenantRepository();
            LoadTenants();
        }

        private void LoadTenants()
        {
            try
            {
                _allTenants = _tenantRepository.GetAll();

                // Create anonymous objects with the correct property names for binding
                var tenantsForDisplay = _allTenants.Select(t => new
                {
                    t.Id,
                    FullName = $"{t.FirstName} {t.LastName}",
                    t.Email,
                    t.Phone,
                    t.Address,
                    t.NationalId,
                    t.EmergencyContact,
                    t.EmergencyPhone
                }).ToList();

                DataGridTenants.ItemsSource = tenantsForDisplay;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tenants: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddTenant_Click(object sender, RoutedEventArgs e)
        {
            var addTenantWindow = new AddEditTenantWindow();
            if (addTenantWindow.ShowDialog() == true)
            {
                LoadTenants();
                MainWindow.NotifyDataChanged(); // Notify main window
            }
        }

        private void BtnEditTenant_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTenants.SelectedItem != null)
            {
                var selectedItem = DataGridTenants.SelectedItem;
                var tenantIdProperty = selectedItem.GetType().GetProperty("Id");
                int tenantId = tenantIdProperty != null ? (int)tenantIdProperty.GetValue(selectedItem) : 0;

                if (tenantId > 0)
                {
                    var tenantToEdit = _allTenants.FirstOrDefault(t => t.Id == tenantId);
                    if (tenantToEdit != null)
                    {
                        var editTenantWindow = new AddEditTenantWindow(tenantToEdit);
                        if (editTenantWindow.ShowDialog() == true)
                        {
                            LoadTenants();
                            MainWindow.NotifyDataChanged(); // Notify main window
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
                int tenantId = tenantIdProperty != null ? (int)tenantIdProperty.GetValue(selectedItem) : 0;

                if (tenantId > 0)
                {
                    var tenantToDelete = _allTenants.FirstOrDefault(t => t.Id == tenantId);
                    if (tenantToDelete != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete tenant {tenantToDelete.FirstName} {tenantToDelete.LastName}?",
                                                   "Confirm Delete",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _tenantRepository.Delete(tenantToDelete.Id);
                                LoadTenants();
                                MainWindow.NotifyDataChanged(); // Notify main window
                                                                // Removed the incomplete comment " // ... rest of method"
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
            var searchText = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadTenants();
            }
            else
            {
                var filteredTenants = _allTenants.Where(t =>
                    (t.FirstName?.ToLower().Contains(searchText) ?? false) ||
                    (t.LastName?.ToLower().Contains(searchText) ?? false) ||
                    (t.Email?.ToLower().Contains(searchText) ?? false) ||
                    (t.Phone?.ToLower().Contains(searchText) ?? false) ||
                    (t.UnitNumber?.ToLower().Contains(searchText) ?? false)).ToList();

                var tenantsForDisplay = filteredTenants.Select(t => new
                {
                    t.Id,
                    FullName = $"{t.FirstName} {t.LastName}",
                    t.FirstName,
                    t.LastName,
                    t.Email,
                    t.Phone,
                    t.UnitNumber,
                    PropertyName = t.PropertyName ?? "No Property",
                    Status = t.IsActive ? "Active" : "Inactive"
                }).ToList();

                DataGridTenants.ItemsSource = tenantsForDisplay;
            }
        }
    }
}