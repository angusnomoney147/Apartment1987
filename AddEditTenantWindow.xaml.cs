using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class AddEditTenantWindow : Window
    {
        private Tenant? _tenant;
        private readonly TenantRepository _tenantRepository;
        private readonly bool _isEditMode;

        // Add these new fields for lease functionality
        private UnitRepository _unitRepository;
        private List<Unit> _units;

        public AddEditTenantWindow()
        {
            InitializeComponent();
            _tenantRepository = new TenantRepository();
            _isEditMode = false;
            TxtHeader.Text = "Add New Tenant";
        }

        public AddEditTenantWindow(Tenant tenant) : this()
        {
            _tenant = tenant;
            _isEditMode = true;
            TxtHeader.Text = "Edit Tenant";
            LoadTenantData();
        }

        private void LoadTenantData()
        {
            if (_tenant != null)
            {
                TxtFirstName.Text = _tenant.FirstName;
                TxtLastName.Text = _tenant.LastName;
                TxtEmail.Text = _tenant.Email;
                TxtPhone.Text = _tenant.Phone;
                DpDateOfBirth.SelectedDate = _tenant.DateOfBirth;
                TxtNationalId.Text = _tenant.NationalId;
                TxtAddress.Text = _tenant.Address;
                TxtEmergencyContact.Text = _tenant.EmergencyContact;
                TxtEmergencyPhone.Text = _tenant.EmergencyPhone;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                if (_tenant == null)
                    _tenant = new Tenant();

                _tenant.FirstName = TxtFirstName.Text.Trim();
                _tenant.LastName = TxtLastName.Text.Trim();
                _tenant.Email = string.IsNullOrWhiteSpace(TxtEmail.Text) ? null : TxtEmail.Text.Trim();
                _tenant.Phone = string.IsNullOrWhiteSpace(TxtPhone.Text) ? null : TxtPhone.Text.Trim();
                _tenant.DateOfBirth = DpDateOfBirth.SelectedDate ?? DateTime.Now;
                _tenant.NationalId = string.IsNullOrWhiteSpace(TxtNationalId.Text) ? null : TxtNationalId.Text.Trim();
                _tenant.Address = string.IsNullOrWhiteSpace(TxtAddress.Text) ? null : TxtAddress.Text.Trim();
                _tenant.EmergencyContact = string.IsNullOrWhiteSpace(TxtEmergencyContact.Text) ? null : TxtEmergencyContact.Text.Trim();
                _tenant.EmergencyPhone = string.IsNullOrWhiteSpace(TxtEmergencyPhone.Text) ? null : TxtEmergencyPhone.Text.Trim();

                if (_isEditMode)
                {
                    _tenantRepository.Update(_tenant);
                    MessageBox.Show("Tenant updated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _tenant.CreatedDate = DateTime.Now;
                    _tenant.IsActive = true;
                    _tenantRepository.Add(_tenant);
                    MessageBox.Show("Tenant added successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Check if user wants to create a lease
                    if (ChkCreateLease.IsChecked == true)
                    {
                        if (ValidateLeaseInput())
                        {
                            CreateLeaseForTenant(_tenant);
                        }
                        else
                        {
                            return; // Don't close the dialog if lease validation fails
                        }
                    }
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tenant: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                MessageBox.Show("First name is required.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtLastName.Text))
            {
                MessageBox.Show("Last name is required.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtLastName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(TxtEmail.Text) && !IsValidEmail(TxtEmail.Text.Trim()))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtEmail.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Add these new methods for lease functionality
        private void ChkCreateLease_Checked(object sender, RoutedEventArgs e)
        {
            LoadUnitsForLease();
            LeaseDetailsGrid.Visibility = Visibility.Visible;
        }

        private void ChkCreateLease_Unchecked(object sender, RoutedEventArgs e)
        {
            LeaseDetailsGrid.Visibility = Visibility.Collapsed;
        }

        private void LoadUnitsForLease()
        {
            try
            {
                _unitRepository = new UnitRepository();
                _units = _unitRepository.GetAll();

                CmbUnits.ItemsSource = _units.Where(u => u.Status == UnitStatus.Vacant)
                                            .Select(u => new { u.Id, Display = $"{u.UnitNumber} - ${u.RentAmount}/month" })
                                            .ToList();
                CmbUnits.DisplayMemberPath = "Display";
                CmbUnits.SelectedValuePath = "Id";

                // Set default dates
                DpStartDate.SelectedDate = DateTime.Now;
                DpEndDate.SelectedDate = DateTime.Now.AddYears(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading units: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this method to validate lease input
        private bool ValidateLeaseInput()
        {
            if (CmbUnits.SelectedValue == null)
            {
                MessageBox.Show("Please select a unit for the lease.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!DpStartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a lease start date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!DpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a lease end date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DpStartDate.SelectedDate >= DpEndDate.SelectedDate)
            {
                MessageBox.Show("Lease end date must be after start date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(TxtMonthlyRent.Text, out _))
            {
                MessageBox.Show("Please enter a valid monthly rent amount.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CreateLeaseForTenant(Tenant tenant)
        {
            try
            {
                var lease = new Lease
                {
                    UnitId = (int)CmbUnits.SelectedValue,
                    TenantId = tenant.Id,
                    StartDate = DpStartDate.SelectedDate ?? DateTime.Now,
                    EndDate = DpEndDate.SelectedDate ?? DateTime.Now.AddYears(1),
                    MonthlyRent = decimal.TryParse(TxtMonthlyRent.Text, out decimal rent) ? rent : 0,
                    SecurityDeposit = decimal.TryParse(TxtSecurityDeposit.Text, out decimal deposit) ? deposit : 0,
                    Terms = "Standard lease terms",
                    Status = LeaseStatus.Active,
                    CreatedDate = DateTime.Now
                };

                var leaseRepository = new LeaseRepository();
                leaseRepository.Add(lease);

                // Generate lease agreement
                var unit = _units.FirstOrDefault(u => u.Id == lease.UnitId);
                var property = unit != null ? new PropertyRepository().GetAll().FirstOrDefault(p => p.Id == unit.PropertyId) : null;

                if (unit != null && property != null)
                {
                    var agreement = LeaseAgreementHelper.GenerateLeaseAgreement(lease, tenant, unit, property);

                    var saveDialog = new Microsoft.Win32.SaveFileDialog
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

                        MessageBox.Show($"Lease created and agreement generated successfully!\nSaved to: {saveDialog.FileName}",
                                      "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating lease: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DecimalOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow numbers and decimal point
            e.Handled = !IsDecimalAllowed(e.Text, ((TextBox)sender).Text);
        }

        private static bool IsDecimalAllowed(string text, string currentText)
        {
            // Allow only one decimal point
            if (text == "." && currentText.Contains("."))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(text, @"^[0-9.]$");
        }
    }
}