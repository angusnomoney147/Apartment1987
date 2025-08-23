using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class PropertyManagementWindow : Window
    {
        private readonly PropertyRepository _propertyRepository;
        private List<Property> _allProperties = new();

        public PropertyManagementWindow()
        {
            InitializeComponent();
            _propertyRepository = new PropertyRepository();
            LoadProperties();
        }

        private void LoadProperties()
        {
            try
            {
                _allProperties = _propertyRepository.GetAll();
                DataGridProperties.ItemsSource = _allProperties;
                TxtStatus.Text = $"Loaded {_allProperties.Count} properties";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this method to notify parent window
        private void NotifyParentOfChanges()
        {
            // Find the main window and tell it to refresh
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.ManualRefresh();
        }

        // Update BtnAddProperty_Click
        private void BtnAddProperty_Click(object sender, RoutedEventArgs e)
        {
            var addPropertyWindow = new AddEditPropertyWindow();
            if (addPropertyWindow.ShowDialog() == true)
            {
                LoadProperties(); // Refresh the list
                TxtStatus.Text = "Property added successfully";
                NotifyParentOfChanges(); // Add this line
            }
        }

        // Update BtnEditProperty_Click
        private void BtnEditProperty_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridProperties.SelectedItem is Property selectedProperty)
            {
                var editPropertyWindow = new AddEditPropertyWindow(selectedProperty);
                if (editPropertyWindow.ShowDialog() == true)
                {
                    LoadProperties(); // Refresh the list
                    TxtStatus.Text = "Property updated successfully";
                    NotifyParentOfChanges(); // Add this line
                }
            }
            else
            {
                MessageBox.Show("Please select a property to edit.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Update BtnDeleteProperty_Click
        private void BtnDeleteProperty_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridProperties.SelectedItem is Property selectedProperty)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedProperty.Name}?",
                                           "Confirm Delete",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _propertyRepository.Delete(selectedProperty.Id);
                        LoadProperties(); // Refresh the list
                        TxtStatus.Text = "Property deleted successfully";
                        NotifyParentOfChanges(); // Add this line
                        MessageBox.Show("Property deleted successfully!", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting property: {ex.Message}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a property to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                DataGridProperties.ItemsSource = _allProperties;
            }
            else
            {
                var filteredProperties = _allProperties.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    (p.Address?.ToLower().Contains(searchText) ?? false) ||
                    (p.City?.ToLower().Contains(searchText) ?? false) ||
                    (p.Country?.ToLower().Contains(searchText) ?? false) ||
                    (p.ManagerName?.ToLower().Contains(searchText) ?? false)).ToList();

                DataGridProperties.ItemsSource = filteredProperties;
            }
        }
    }
}