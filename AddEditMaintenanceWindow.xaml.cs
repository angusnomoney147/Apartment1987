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
        private MaintenanceRequest? _request;
        private readonly MaintenanceRepository _maintenanceRepository;
        private readonly List<Unit> _units;
        private readonly List<Tenant> _tenants;
        private readonly bool _isEditMode;

        public AddEditMaintenanceWindow(List<Unit> units, List<Tenant> tenants)
        {
            InitializeComponent();
            _maintenanceRepository = new MaintenanceRepository();
            _units = units;
            _tenants = tenants;
            _isEditMode = false;
            TxtHeader.Text = "➕ Add New Maintenance Request";
            InitializeForm();
        }

        public AddEditMaintenanceWindow(MaintenanceRequest request, List<Unit> units, List<Tenant> tenants) : this(units, tenants)
        {
            _request = request;
            _isEditMode = true;
            TxtHeader.Text = "✏️ Edit Maintenance Request";
            LoadRequestData();
        }

        private void InitializeForm()
        {
            // Populate units dropdown with property information
            var unitInfoList = _units.Select(u => new
            {
                u.Id,
                Display = $"{GetPropertyForUnit(u.Id)} - {u.UnitNumber}"
            }).ToList();

            CmbUnits.ItemsSource = unitInfoList;
            CmbUnits.DisplayMemberPath = "Display";
            CmbUnits.SelectedValuePath = "Id";

            // Populate tenants dropdown
            CmbTenants.ItemsSource = _tenants.Select(t => new { t.Id, Display = t.FullName }).ToList();
            CmbTenants.DisplayMemberPath = "Display";
            CmbTenants.SelectedValuePath = "Id";

            // Populate priority dropdown
            var priorities = Enum.GetValues(typeof(MaintenancePriority)).Cast<MaintenancePriority>()
                .Select(p => new { Value = (int)p, Name = MaintenancePriorityHelper.GetPriorityName(p) })
                .ToList();

            CmbPriority.ItemsSource = priorities;
            CmbPriority.DisplayMemberPath = "Name";
            CmbPriority.SelectedValuePath = "Value";
            CmbPriority.SelectedIndex = 1; // Default to Medium

            // Populate status dropdown
            var statuses = Enum.GetValues(typeof(MaintenanceStatus)).Cast<MaintenanceStatus>()
                .Select(s => new { Value = (int)s, Name = MaintenanceStatusHelper.GetStatusName(s) })
                .ToList();

            CmbStatus.ItemsSource = statuses;
            CmbStatus.DisplayMemberPath = "Name";
            CmbStatus.SelectedValuePath = "Value";
            CmbStatus.SelectedIndex = 0; // Default to Pending

            // Set default dates
            DpRequestDate.SelectedDate = DateTime.Now;
        }

        private string GetPropertyForUnit(int unitId)
        {
            var unit = _units.FirstOrDefault(u => u.Id == unitId);
            if (unit != null)
            {
                var property = new PropertyRepository().GetAll().FirstOrDefault(p => p.Id == unit.PropertyId);
                return property?.Name ?? "Unknown Property";
            }
            return "Unknown Property";
        }
        private void LoadRequestData()
        {
            if (_request != null)
            {
                CmbUnits.SelectedValue = _request.UnitId;
                CmbTenants.SelectedValue = _request.TenantId;
                TxtDescription.Text = _request.Description;
                CmbPriority.SelectedValue = (int)_request.Priority;
                CmbStatus.SelectedValue = (int)_request.Status;
                DpRequestDate.SelectedDate = _request.RequestDate;
                DpCompletedDate.SelectedDate = _request.CompletedDate;
                TxtCost.Text = _request.Cost?.ToString("F2");
                TxtAssignedTo.Text = _request.AssignedTo;
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
                _request.TenantId = (int)CmbTenants.SelectedValue;
                _request.Description = TxtDescription.Text.Trim();
                _request.Priority = (MaintenancePriority)CmbPriority.SelectedValue;
                _request.Status = (MaintenanceStatus)CmbStatus.SelectedValue;
                _request.RequestDate = DpRequestDate.SelectedDate ?? DateTime.Now;
                _request.CompletedDate = DpCompletedDate.SelectedDate;
                _request.Cost = decimal.TryParse(TxtCost.Text, out decimal cost) ? cost : (decimal?)null;
                _request.AssignedTo = string.IsNullOrWhiteSpace(TxtAssignedTo.Text) ? null : TxtAssignedTo.Text.Trim();

                if (_isEditMode)
                {
                    _maintenanceRepository.Update(_request);
                    MessageBox.Show("Maintenance request updated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _maintenanceRepository.Add(_request);
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

            if (CmbTenants.SelectedValue == null)
            {
                MessageBox.Show("Please select a tenant.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbTenants.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtDescription.Text))
            {
                MessageBox.Show("Please enter a description.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDescription.Focus();
                return false;
            }

            if (!DpRequestDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a request date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DpRequestDate.Focus();
                return false;
            }

            return true;
        }

        private void UpdateUnitStatusAfterMaintenance(int unitId)
        {
            try
            {
                var unitRepository = new UnitRepository();
                var unit = unitRepository.GetAll().FirstOrDefault(u => u.Id == unitId);

                if (unit != null && unit.Status == UnitStatus.Maintenance)
                {
                    // Check if there are any other pending maintenance requests for this unit
                    var maintenanceRepository = new MaintenanceRepository();
                    var pendingRequests = maintenanceRepository.GetAll()
                        .Where(r => r.UnitId == unitId &&
                                   (r.Status == MaintenanceStatus.Pending || r.Status == MaintenanceStatus.InProgress))
                        .ToList();

                    if (!pendingRequests.Any())
                    {
                        // No more pending maintenance, update unit to Vacant
                        var oldStatus = unit.Status;
                        unit.Status = UnitStatus.Vacant;
                        unitRepository.Update(unit);

                        // Log the status change
                        MessageBox.Show($"Unit {unit.UnitNumber} has been automatically updated to Vacant status as all maintenance is completed.",
                                      "Unit Status Updated",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating unit status: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbStatus.SelectedValue != null)
            {
                var selectedStatus = (MaintenanceStatus)(int)CmbStatus.SelectedValue;

                // If status is changed to Completed, auto-fill completion date
                if (selectedStatus == MaintenanceStatus.Completed && !DpCompletedDate.SelectedDate.HasValue)
                {
                    DpCompletedDate.SelectedDate = DateTime.Now;
                }

                // Show warning if changing from Completed back to other status
                if (_isEditMode && _request?.Status == MaintenanceStatus.Completed && selectedStatus != MaintenanceStatus.Completed)
                {
                    MessageBox.Show("Note: Changing status from Completed back to another status may affect unit availability tracking.",
                                  "Status Change Warning",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void DecimalOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow numbers and decimal point
            e.Handled = !IsDecimalAllowed(e.Text, ((TextBox)sender).Text);
        }

        private static bool IsDecimalAllowed(string text, string currentText)
        {
            // Allow only one decimal point
            if (text == "." && currentText.Contains("."))
                return false;

            return Regex.IsMatch(text, @"^[0-9.]$");
        }
    }
}