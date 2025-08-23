using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class UnitManagementWindow : Window
    {
        private readonly UnitRepository _unitRepository;
        private readonly PropertyRepository _propertyRepository;
        private List<Unit> _allUnits = new();
        private List<Property> _allProperties = new();

        public UnitManagementWindow()
        {
            InitializeComponent();
            _unitRepository = new UnitRepository();
            _propertyRepository = new PropertyRepository();
            LoadProperties();
        }

        private void LoadProperties()
        {
            try
            {
                _allProperties = _propertyRepository.GetAll();
                CmbProperties.ItemsSource = _allProperties.Select(p => new { p.Id, Display = p.Name }).ToList();
                CmbProperties.DisplayMemberPath = "Display";
                CmbProperties.SelectedValuePath = "Id";

                if (_allProperties.Any())
                {
                    CmbProperties.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUnits(int propertyId)
        {
            try
            {
                _allUnits = _unitRepository.GetUnitsByProperty(propertyId);
                var unitsWithStatusNames = _allUnits.Select(u => new
                {
                    u.Id,
                    u.UnitNumber,
                    u.UnitType,
                    u.Size,
                    u.RentAmount,
                    u.Bedrooms,
                    u.Bathrooms,
                    Status = UnitStatusHelper.GetStatusName(u.Status),
                    u.Description
                }).ToList();

                DataGridUnits.ItemsSource = unitsWithStatusNames;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading units: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbProperties.SelectedItem != null)
            {
                int propertyId = (int)CmbProperties.SelectedValue;
                LoadUnits(propertyId);
            }
        }

        private void NotifyParentOfChanges()
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.ManualRefresh();
        }

        private void BtnAddUnit_Click(object sender, RoutedEventArgs e)
        {
            if (CmbProperties.SelectedValue != null)
            {
                int propertyId = (int)CmbProperties.SelectedValue;
                var addUnitWindow = new AddEditUnitWindow(propertyId);
                if (addUnitWindow.ShowDialog() == true)
                {
                    LoadUnits(propertyId);
                    NotifyParentOfChanges();
                }
            }
        }

        private void BtnEditUnit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridUnits.SelectedItem != null)
            {
                var selectedItem = DataGridUnits.SelectedItem;
                var unitIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedUnitId = unitIdProperty != null ? (int)unitIdProperty.GetValue(selectedItem) : 0;

                if (selectedUnitId > 0)
                {
                    var unitToEdit = _allUnits.FirstOrDefault(u => u.Id == selectedUnitId);
                    if (unitToEdit != null)
                    {
                        var editUnitWindow = new AddEditUnitWindow(unitToEdit);
                        if (editUnitWindow.ShowDialog() == true)
                        {
                            if (CmbProperties.SelectedValue != null)
                            {
                                int propertyId = (int)CmbProperties.SelectedValue;
                                LoadUnits(propertyId);
                            }
                            NotifyParentOfChanges();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a unit to edit.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteUnit_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridUnits.SelectedItem != null)
            {
                var selectedItem = DataGridUnits.SelectedItem;
                var unitIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedUnitId = unitIdProperty != null ? (int)unitIdProperty.GetValue(selectedItem) : 0;

                if (selectedUnitId > 0)
                {
                    var unitToDelete = _allUnits.FirstOrDefault(u => u.Id == selectedUnitId);
                    if (unitToDelete != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete Unit {unitToDelete.UnitNumber}?",
                                                   "Confirm Delete",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _unitRepository.Delete(unitToDelete.Id);
                                if (CmbProperties.SelectedValue != null)
                                {
                                    int propertyId = (int)CmbProperties.SelectedValue;
                                    LoadUnits(propertyId);
                                }
                                NotifyParentOfChanges();
                                MessageBox.Show("Unit deleted successfully!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting unit: {ex.Message}", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a unit to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allUnits == null || CmbProperties.SelectedItem == null) return;

            var propertyId = (int)CmbProperties.SelectedValue;
            var searchText = TxtSearch.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadUnits(propertyId);
            }
            else
            {
                var filteredUnits = _allUnits.Where(u =>
                    u.UnitNumber.ToLower().Contains(searchText) ||
                    (u.UnitType?.ToLower().Contains(searchText) ?? false) ||
                    (u.Description?.ToLower().Contains(searchText) ?? false)).ToList();

                var unitsWithStatusNames = filteredUnits.Select(u => new
                {
                    u.Id,
                    u.UnitNumber,
                    u.UnitType,
                    u.Size,
                    u.RentAmount,
                    u.Bedrooms,
                    u.Bathrooms,
                    Status = UnitStatusHelper.GetStatusName(u.Status),
                    u.Description
                }).ToList();

                DataGridUnits.ItemsSource = unitsWithStatusNames;
            }
        }

        private void BtnRefreshProperties_Click(object sender, RoutedEventArgs e)
        {
            LoadProperties();
        }
    }
}