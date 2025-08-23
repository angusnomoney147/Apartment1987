using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace ApartmentManagementSystem
{
    public partial class AddEditLeaseWindow : Window
    {
        private Lease? _lease;
        private readonly LeaseRepository _leaseRepository;
        private readonly PropertyRepository _propertyRepository;
        private readonly UnitRepository _unitRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly List<Property> _properties;
        private readonly List<Unit> _units;
        private readonly List<Tenant> _tenants;
        private readonly bool _isEditMode;

        public AddEditLeaseWindow(List<Property> properties, List<Unit> units, List<Tenant> tenants)
        {
            InitializeComponent();
            _leaseRepository = new LeaseRepository();
            _propertyRepository = new PropertyRepository();
            _unitRepository = new UnitRepository();
            _tenantRepository = new TenantRepository();
            _properties = properties;
            _units = units;
            _tenants = tenants;
            _isEditMode = false;
            TxtHeader.Text = "Add New Lease";
            InitializeForm(); // THIS WAS MISSING!
        }

        public AddEditLeaseWindow(Lease lease, List<Property> properties, List<Unit> units, List<Tenant> tenants) : this(properties, units, tenants)
        {
            _lease = lease;
            _isEditMode = true;
            TxtHeader.Text = "Edit Lease";
            LoadLeaseData();
        }

        private void InitializeForm()
        {
            // Populate properties dropdown
            CmbProperties.ItemsSource = _properties.Select(p => new { p.Id, Display = p.Name }).ToList();
            CmbProperties.DisplayMemberPath = "Display";
            CmbProperties.SelectedValuePath = "Id";

            // Populate tenants dropdown
            CmbTenants.ItemsSource = _tenants.Select(t => new { t.Id, Display = t.FullName }).ToList();
            CmbTenants.DisplayMemberPath = "Display";
            CmbTenants.SelectedValuePath = "Id";

            // Populate status dropdown
            var statuses = Enum.GetValues(typeof(LeaseStatus)).Cast<LeaseStatus>()
                .Select(s => new { Value = (int)s, Name = LeaseStatusHelper.GetStatusName(s) })
                .ToList();

            CmbStatus.ItemsSource = statuses;
            CmbStatus.DisplayMemberPath = "Name";
            CmbStatus.SelectedValuePath = "Value";
            CmbStatus.SelectedIndex = 0; // Default to Active

            // Set default dates
            DpStartDate.SelectedDate = DateTime.Now;
            DpEndDate.SelectedDate = DateTime.Now.AddYears(1);
        }

        private void LoadLeaseData()
        {
            if (_lease != null)
            {
                // Find property for this unit
                var unit = _units.FirstOrDefault(u => u.Id == _lease.UnitId);
                if (unit != null)
                {
                    CmbProperties.SelectedValue = unit.PropertyId;
                    LoadUnitsForProperty(unit.PropertyId);
                    CmbUnits.SelectedValue = _lease.UnitId;
                }

                CmbTenants.SelectedValue = _lease.TenantId;
                DpStartDate.SelectedDate = _lease.StartDate;
                DpEndDate.SelectedDate = _lease.EndDate;
                TxtMonthlyRent.Text = _lease.MonthlyRent.ToString("F2");
                TxtSecurityDeposit.Text = _lease.SecurityDeposit.ToString("F2");
                TxtTerms.Text = _lease.Terms;
                CmbStatus.SelectedValue = (int)_lease.Status;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                if (_lease == null)
                    _lease = new Lease();

                _lease.UnitId = (int)CmbUnits.SelectedValue;
                _lease.TenantId = (int)CmbTenants.SelectedValue;
                _lease.StartDate = DpStartDate.SelectedDate ?? DateTime.Now;
                _lease.EndDate = DpEndDate.SelectedDate ?? DateTime.Now.AddYears(1);
                _lease.MonthlyRent = decimal.TryParse(TxtMonthlyRent.Text, out decimal monthlyRent) ? monthlyRent : 0;
                _lease.SecurityDeposit = decimal.TryParse(TxtSecurityDeposit.Text, out decimal securityDeposit) ? securityDeposit : 0;
                _lease.Terms = string.IsNullOrWhiteSpace(TxtTerms.Text) ? null : TxtTerms.Text.Trim();
                _lease.Status = (LeaseStatus)CmbStatus.SelectedValue;
                _lease.CreatedDate = _isEditMode ? _lease.CreatedDate : DateTime.Now;

                if (_isEditMode)
                {
                    _leaseRepository.Update(_lease);
                    MessageBox.Show("Lease updated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _leaseRepository.Add(_lease);

                    // AUTOMATICALLY UPDATE UNIT STATUS TO OCCUPIED
                    UpdateUnitStatusToOccupied(_lease.UnitId);

                    MessageBox.Show("Lease added successfully! Unit status automatically updated to Occupied.", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ask if they want to generate lease agreement
                    var result = MessageBox.Show("Would you like to generate a lease agreement?",
                                               "Generate Agreement",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        GenerateLeaseAgreement(_lease);
                    }
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving lease: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this new method for automatic unit status update
        private void UpdateUnitStatusToOccupied(int unitId)
        {
            try
            {
                var unitRepository = new UnitRepository();
                var unit = unitRepository.GetAll().FirstOrDefault(u => u.Id == unitId);

                if (unit != null && unit.Status != UnitStatus.Occupied)
                {
                    var oldStatus = unit.Status;
                    unit.Status = UnitStatus.Occupied;
                    unitRepository.Update(unit);

                    // Log the status change in recent activity
                    LogUnitStatusChange(unit.UnitNumber, oldStatus, UnitStatus.Occupied);
                }
            }
            catch (Exception ex)
            {
                // Log error silently - don't interrupt the main flow
                System.Diagnostics.Debug.WriteLine($"Error updating unit status: {ex.Message}");
            }
        }

        private void LogUnitStatusChange(string unitNumber, UnitStatus oldStatus, UnitStatus newStatus)
        {
            try
            {
                // This would typically go to a logs table, but for now we can show a notification
                System.Diagnostics.Debug.WriteLine($"Unit {unitNumber} status changed from {UnitStatusHelper.GetStatusName(oldStatus)} to {UnitStatusHelper.GetStatusName(newStatus)}");
            }
            catch
            {
                // Silent fail
            }
        }

        private void CmbProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbProperties.SelectedValue != null)
            {
                int propertyId = (int)CmbProperties.SelectedValue;
                LoadUnitsForProperty(propertyId);
            }
        }

        private void LoadUnitsForProperty(int propertyId)
        {
            var unitsForProperty = _units.Where(u => u.PropertyId == propertyId && u.Status == UnitStatus.Vacant).ToList();

            // If editing and the unit belongs to this property, include it even if occupied
            if (_isEditMode && _lease != null)
            {
                var currentUnit = _units.FirstOrDefault(u => u.Id == _lease.UnitId);
                if (currentUnit != null && currentUnit.PropertyId == propertyId)
                {
                    unitsForProperty = _units.Where(u => u.PropertyId == propertyId &&
                                                       (u.Status == UnitStatus.Vacant || u.Id == _lease.UnitId)).ToList();
                }
            }

            CmbUnits.ItemsSource = unitsForProperty.Select(u => new { u.Id, Display = $"{u.UnitNumber} - ${u.RentAmount}/month" }).ToList();
            CmbUnits.DisplayMemberPath = "Display";
            CmbUnits.SelectedValuePath = "Id";

            // If editing, select the current unit
            if (_isEditMode && _lease != null)
            {
                var currentUnit = _units.FirstOrDefault(u => u.Id == _lease.UnitId);
                if (currentUnit != null && currentUnit.PropertyId == propertyId)
                {
                    CmbUnits.SelectedValue = _lease.UnitId;
                }
            }
        }

        private bool ValidateInput()
        {
            if (CmbProperties.SelectedValue == null)
            {
                MessageBox.Show("Please select a property.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbProperties.Focus();
                return false;
            }

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

            if (!DpStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a start date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DpStartDate.Focus();
                return false;
            }

            if (!DpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select an end date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DpEndDate.Focus();
                return false;
            }

            if (DpStartDate.SelectedDate >= DpEndDate.SelectedDate)
            {
                MessageBox.Show("End date must be after start date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DpEndDate.Focus();
                return false;
            }

            if (!decimal.TryParse(TxtMonthlyRent.Text, out _))
            {
                MessageBox.Show("Please enter a valid monthly rent.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMonthlyRent.Focus();
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

        // Add this new method for lease agreement generation
        private void GenerateLeaseAgreement(Lease lease)
        {
            try
            {
                // Get related data
                var unit = _units.FirstOrDefault(u => u.Id == lease.UnitId);
                var tenant = _tenants.FirstOrDefault(t => t.Id == lease.TenantId);
                var property = unit != null ? _properties.FirstOrDefault(p => p.Id == unit.PropertyId) : null;

                if (unit != null && tenant != null && property != null)
                {
                    var agreement = LeaseAgreementHelper.GenerateLeaseAgreement(lease, tenant, unit, property);

                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Text Files (*.txt)|*.txt|PDF Files (*.pdf)|*.pdf",
                        FileName = $"Lease_Agreement_{unit.UnitNumber}_{DateTime.Now:yyyyMMdd}.txt"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (saveDialog.FileName.EndsWith(".pdf"))
                        {
                            LeaseAgreementHelper.ExportLeaseAgreementToPdf(agreement, saveDialog.FileName);
                        }
                        else
                        {
                            LeaseAgreementHelper.SaveLeaseAgreement(agreement, saveDialog.FileName);
                        }

                        MessageBox.Show($"Lease agreement generated successfully!\nSaved to: {saveDialog.FileName}",
                                      "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating lease agreement: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnViewDocuments_Click(object sender, RoutedEventArgs e)
        {
            if (_lease != null || !_isEditMode)
            {
                Lease leaseToUse;
                Unit unitToUse;
                Tenant tenantToUse;

                if (_isEditMode && _lease != null)
                {
                    // Editing existing lease
                    leaseToUse = _lease;
                    unitToUse = _units.FirstOrDefault(u => u.Id == _lease.UnitId);
                    tenantToUse = _tenants.FirstOrDefault(t => t.Id == _lease.TenantId);
                }
                else if (CmbUnits.SelectedValue != null && CmbTenants.SelectedValue != null)
                {
                    // Creating new lease - create a temporary lease object
                    leaseToUse = new Lease
                    {
                        Id = 0, // Temporary ID
                        UnitId = (int)CmbUnits.SelectedValue,
                        TenantId = (int)CmbTenants.SelectedValue,
                        StartDate = DpStartDate.SelectedDate ?? DateTime.Now,
                        EndDate = DpEndDate.SelectedDate ?? DateTime.Now.AddYears(1),
                        MonthlyRent = decimal.TryParse(TxtMonthlyRent.Text, out decimal rent) ? rent : 0
                    };

                    unitToUse = _units.FirstOrDefault(u => u.Id == leaseToUse.UnitId);
                    tenantToUse = _tenants.FirstOrDefault(t => t.Id == leaseToUse.TenantId);
                }
                else
                {
                    MessageBox.Show("Please select a unit and tenant first.", "Information",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (leaseToUse != null && unitToUse != null && tenantToUse != null)
                {
                    var documentsWindow = new LeaseDocumentsWindow(leaseToUse, unitToUse, tenantToUse);
                    documentsWindow.Show();
                }
            }
        }
    }
}