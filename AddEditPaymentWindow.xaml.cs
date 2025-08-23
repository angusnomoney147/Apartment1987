using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApartmentManagementSystem
{
    public partial class AddEditPaymentWindow : Window
    {
        private Payment? _payment;
        private readonly PaymentRepository _paymentRepository;
        private readonly List<Lease> _leases;
        private readonly List<Unit> _units;
        private readonly List<Tenant> _tenants;
        private readonly List<Property> _properties;
        private readonly bool _isEditMode;

        public AddEditPaymentWindow(List<Lease> leases, List<Unit> units, List<Tenant> tenants, List<Property> properties)
        {
            InitializeComponent();
            _paymentRepository = new PaymentRepository();
            _leases = leases;
            _units = units;
            _tenants = tenants;
            _properties = properties;
            _isEditMode = false;
            TxtHeader.Text = "Add New Payment";
            InitializeForm();
        }

        public AddEditPaymentWindow(Payment payment, List<Lease> leases, List<Unit> units, List<Tenant> tenants, List<Property> properties) : this(leases, units, tenants, properties)
        {
            _payment = payment;
            _isEditMode = true;
            TxtHeader.Text = "Edit Payment";
            LoadPaymentData();
        }

        private void InitializeForm()
        {
            // Populate leases dropdown with detailed lease info
            var leaseInfoList = _leases.Select(l => new
            {
                l.Id,
                Display = GetLeaseDisplay(l)
            }).ToList();

            CmbLeases.ItemsSource = leaseInfoList;
            CmbLeases.DisplayMemberPath = "Display";
            CmbLeases.SelectedValuePath = "Id";

            // Populate method dropdown
            var methods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                .Select(m => new { Value = (int)m, Name = PaymentMethodHelper.GetMethodName(m) })
                .ToList();

            CmbMethod.ItemsSource = methods;
            CmbMethod.DisplayMemberPath = "Name";
            CmbMethod.SelectedValuePath = "Value";
            CmbMethod.SelectedIndex = 0; // Default to Cash

            // Populate status dropdown
            var statuses = Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>()
                .Select(s => new { Value = (int)s, Name = PaymentStatusHelper.GetStatusName(s) })
                .ToList();

            CmbStatus.ItemsSource = statuses;
            CmbStatus.DisplayMemberPath = "Name";
            CmbStatus.SelectedValuePath = "Value";
            CmbStatus.SelectedIndex = 0; // Default to Paid

            // Set default payment date
            DpPaymentDate.SelectedDate = DateTime.Now;
        }

        private string GetLeaseDisplay(Lease lease)
        {
            // Get related data
            var unit = _units.FirstOrDefault(u => u.Id == lease.UnitId);
            var tenant = _tenants.FirstOrDefault(t => t.Id == lease.TenantId);
            var property = unit != null ? _properties.FirstOrDefault(p => p.Id == unit.PropertyId) : null;

            // Create detailed display string
            var propertyName = property?.Name ?? "Unknown Property";
            var unitNumber = unit?.UnitNumber ?? "Unknown Unit";
            var tenantName = tenant?.FullName ?? "Unknown Tenant";

            return $"{propertyName}, {unitNumber}, {tenantName} (${lease.MonthlyRent}/month)";
        }

        private void LoadPaymentData()
        {
            if (_payment != null)
            {
                CmbLeases.SelectedValue = _payment.LeaseId;
                TxtAmount.Text = _payment.Amount.ToString("F2");
                DpPaymentDate.SelectedDate = _payment.PaymentDate;
                CmbMethod.SelectedValue = (int)_payment.Method;
                TxtReferenceNumber.Text = _payment.ReferenceNumber;
                CmbStatus.SelectedValue = (int)_payment.Status;
                TxtNotes.Text = _payment.Notes;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                if (_payment == null)
                    _payment = new Payment();

                _payment.LeaseId = (int)CmbLeases.SelectedValue;
                _payment.Amount = decimal.TryParse(TxtAmount.Text, out decimal amount) ? amount : 0;
                _payment.PaymentDate = DpPaymentDate.SelectedDate ?? DateTime.Now;
                _payment.Method = (PaymentMethod)CmbMethod.SelectedValue;
                _payment.ReferenceNumber = string.IsNullOrWhiteSpace(TxtReferenceNumber.Text) ? null : TxtReferenceNumber.Text.Trim();
                _payment.Status = (PaymentStatus)CmbStatus.SelectedValue;
                _payment.Notes = string.IsNullOrWhiteSpace(TxtNotes.Text) ? null : TxtNotes.Text.Trim();

                if (_isEditMode)
                {
                    _paymentRepository.Update(_payment);
                    MessageBox.Show("Payment updated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _paymentRepository.Add(_payment);
                    MessageBox.Show("Payment added successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (CmbLeases.SelectedValue == null)
            {
                MessageBox.Show("Please select a lease.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbLeases.Focus();
                return false;
            }

            if (!decimal.TryParse(TxtAmount.Text, out _))
            {
                MessageBox.Show("Please enter a valid amount.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtAmount.Focus();
                return false;
            }

            if (!DpPaymentDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a payment date.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DpPaymentDate.Focus();
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
    }
}