using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApartmentManagementSystem
{
    public partial class AddEditUnitWindow : Window
    {
        private Unit? _unit;
        private readonly UnitRepository _unitRepository;
        private readonly int _propertyId;
        private readonly bool _isEditMode;

        public AddEditUnitWindow(int propertyId)
        {
            InitializeComponent();
            _unitRepository = new UnitRepository();
            _propertyId = propertyId;
            _isEditMode = false;
            TxtHeader.Text = "➕ Add New Unit";
            InitializeForm();
        }

        public AddEditUnitWindow(Unit unit) : this(unit.PropertyId)
        {
            _unit = unit;
            _isEditMode = true;
            TxtHeader.Text = "✏️ Edit Unit";
            LoadUnitData();
        }

        private void InitializeForm()
        {
            // Populate status dropdown
            CmbStatus.ItemsSource = Enum.GetValues(typeof(UnitStatus));
            CmbStatus.SelectedIndex = 0; // Default to Vacant
        }

        private void LoadUnitData()
        {
            if (_unit != null)
            {
                TxtUnitNumber.Text = _unit.UnitNumber;
                TxtUnitType.Text = _unit.UnitType;
                TxtSize.Text = _unit.Size.ToString("F2");
                TxtRentAmount.Text = _unit.RentAmount.ToString("F2");
                TxtBedrooms.Text = _unit.Bedrooms.ToString();
                TxtBathrooms.Text = _unit.Bathrooms.ToString();
                TxtDescription.Text = _unit.Description;
                CmbStatus.SelectedItem = _unit.Status;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                if (_unit == null)
                    _unit = new Unit();

                _unit.PropertyId = _propertyId;
                _unit.UnitNumber = TxtUnitNumber.Text.Trim();
                _unit.UnitType = string.IsNullOrWhiteSpace(TxtUnitType.Text) ? null : TxtUnitType.Text.Trim();
                _unit.Size = decimal.TryParse(TxtSize.Text, out decimal size) ? size : 0;
                _unit.RentAmount = decimal.TryParse(TxtRentAmount.Text, out decimal rent) ? rent : 0;
                _unit.Bedrooms = int.TryParse(TxtBedrooms.Text, out int bedrooms) ? bedrooms : 0;
                _unit.Bathrooms = int.TryParse(TxtBathrooms.Text, out int bathrooms) ? bathrooms : 0;
                _unit.Description = string.IsNullOrWhiteSpace(TxtDescription.Text) ? null : TxtDescription.Text.Trim();
                _unit.Status = (UnitStatus)CmbStatus.SelectedItem;

                if (_isEditMode)
                {
                    _unitRepository.Update(_unit);
                    MessageBox.Show("Unit updated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _unitRepository.Add(_unit);
                    MessageBox.Show("Unit added successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving unit: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtUnitNumber.Text))
            {
                MessageBox.Show("Unit number is required.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtUnitNumber.Focus();
                return false;
            }

            if (!decimal.TryParse(TxtSize.Text, out _))
            {
                MessageBox.Show("Please enter a valid size.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSize.Focus();
                return false;
            }

            if (!decimal.TryParse(TxtRentAmount.Text, out _))
            {
                MessageBox.Show("Please enter a valid rent amount.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtRentAmount.Focus();
                return false;
            }

            if (!int.TryParse(TxtBedrooms.Text, out _))
            {
                MessageBox.Show("Please enter a valid number of bedrooms.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtBedrooms.Focus();
                return false;
            }

            if (!int.TryParse(TxtBathrooms.Text, out _))
            {
                MessageBox.Show("Please enter a valid number of bathrooms.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtBathrooms.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numbers
            e.Handled = !IsNumberAllowed(e.Text);
        }

        private void DecimalOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow numbers and decimal point
            e.Handled = !IsDecimalAllowed(e.Text, ((TextBox)sender).Text);
        }

        private static bool IsNumberAllowed(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
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