using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApartmentManagementSystem
{
    public partial class AddEditMaintenanceWindow : Window
    {
        private MaintenanceRequest _request;
        private readonly UnitRepository _unitRepository;
        private readonly bool _isEditMode;
        private List<Unit> _units;

        public AddEditMaintenanceWindow()
        {
            InitializeComponent();
            _unitRepository = new UnitRepository();
            _isEditMode = false;
            TxtHeader.Text = "🔧 Add New Maintenance Request";
            _units = _unitRepository.GetAll();
            InitializeForm();
        }

        public AddEditMaintenanceWindow(MaintenanceRequest request) : this()
        {
            _request = request;
            _isEditMode = true;
            TxtHeader.Text = "🔧 Edit Maintenance Request";
            LoadRequestData();
        }

        private void InitializeForm()
        {
            // Populate units dropdown
            var unitsWithInfo = _units.Select(u => new {
                u.Id,
                Display = $"{u.UnitNumber} - {u.UnitType}"
            }).ToList();
            CmbUnits.ItemsSource = unitsWithInfo;
            CmbUnits.DisplayMemberPath = "Display";
            CmbUnits.SelectedValuePath = "Id";

            // Populate priority dropdown
            CmbPriority.ItemsSource = Enum.GetValues(typeof(MaintenancePriority));
            CmbPriority.SelectedIndex = 1; // Default to Medium

            // Populate status dropdown
            CmbStatus.ItemsSource = Enum.GetValues(typeof(MaintenanceStatus));
            CmbStatus.SelectedIndex = 0; // Default to Pending

            // Set default dates
            DpRequestDate.SelectedDate = DateTime.Now;

            // Enable completed date when status is completed
            CmbStatus.SelectionChanged += CmbStatus_SelectionChanged;
        }

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbStatus.SelectedItem != null)
            {
                var status = (MaintenanceStatus)CmbStatus.SelectedItem;
                DpCompletedDate.IsEnabled = status == MaintenanceStatus.Completed;
                if (status == MaintenanceStatus.Completed && !DpCompletedDate.SelectedDate.HasValue)
                {
                    DpCompletedDate.SelectedDate = DateTime.Now;
                }
            }
        }

        private void LoadRequestData()
        {
            if (_request != null)
            {
                CmbUnits.SelectedValue = _request.UnitId;
                TxtDescription.Text = _request.Description;
                CmbPriority.SelectedItem = _request.Priority;
                CmbStatus.SelectedItem = _request.Status;
                TxtAssignedTo.Text = _request.AssignedTo;
                TxtCost.Text = _request.Cost.HasValue ? _request.Cost.Value.ToString("F2") : "";
                DpRequestDate.SelectedDate = _request.RequestDate;
                DpCompletedDate.SelectedDate = _request.CompletedDate;

                // Enable completed date if status is completed
                DpCompletedDate.IsEnabled = _request.Status == MaintenanceStatus.Completed;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                if (_request == null)
                    _request = new MaintenanceRequest();

                _request.UnitId = (int)CmbUnits.SelectedValue;
                var selectedUnit = _units.FirstOrDefault(u => u.Id == _request.UnitId);
                if (selectedUnit != null)
                {
                    _request.UnitNumber = selectedUnit.UnitNumber;
                    // You might want to get property name from a property repository
                    _request.PropertyName = "Property Name"; // Replace with actual property name
                    _request.TenantName = "Tenant Name"; // Replace with actual tenant name
                }

                _request.Description = TxtDescription.Text.Trim();
                _request.Priority = (MaintenancePriority)CmbPriority.SelectedItem;
                _request.Status = (MaintenanceStatus)CmbStatus.SelectedItem;
                _request.AssignedTo = TxtAssignedTo.Text.Trim();
                _request.Cost = decimal.TryParse(TxtCost.Text, out decimal cost) ? cost : (decimal?)null;
                _request.RequestDate = DpRequestDate.SelectedDate ?? DateTime.Now;
                _request.CompletedDate = DpCompletedDate.IsEnabled ? DpCompletedDate.SelectedDate : null;

                var repository = new MaintenanceRequestRepository();
                if (_isEditMode)
                {
                    repository.Update(_request);
                    MessageBox.Show("Maintenance request updated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    repository.Add(_request);
                    MessageBox.Show("Maintenance request added successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving maintenance request: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (CmbUnits.SelectedValue == null)
            {
                MessageBox.Show("Please select a unit.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbUnits.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtDescription.Text))
            {
                MessageBox.Show("Description is required.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDescription.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TxtCost.Text) && !decimal.TryParse(TxtCost.Text, out _))
            {
                MessageBox.Show("Please enter a valid cost.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCost.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void DecimalOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDecimalAllowed(e.Text, ((TextBox)sender).Text);
        }

        private static bool IsDecimalAllowed(string text, string currentText)
        {
            if (text == "." && currentText.Contains("."))
                return false;

            return Regex.IsMatch(text, @"^[0-9.]$");
        }
    }
}