using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApartmentManagementSystem
{
    public partial class AddEditPropertyWindow : Window
    {
        private Property _property;
        private readonly PropertyRepository _propertyRepository;
        private readonly bool _isEditMode;

        public AddEditPropertyWindow()
        {
            InitializeComponent();
            _propertyRepository = new PropertyRepository();
            _isEditMode = false;
            TxtHeader.Text = "➕ Add New Property";
            InitializeForm();
        }

        public AddEditPropertyWindow(Property property) : this()
        {
            _property = property;
            _isEditMode = true;
            TxtHeader.Text = "✏️ Edit Property";
            LoadPropertyData();
        }

        private void InitializeForm()
        {
            DpCreatedDate.SelectedDate = DateTime.Now;
        }

        private void LoadPropertyData()
        {
            if (_property != null)
            {
                TxtName.Text = _property.Name;
                TxtAddress.Text = _property.Address;
                TxtCity.Text = _property.City;
                TxtState.Text = _property.State;
                TxtZipCode.Text = _property.ZipCode;
                TxtCountry.Text = _property.Country;
                TxtManagerName.Text = _property.ManagerName;
                TxtContactInfo.Text = _property.ContactInfo;
                TxtTotalUnits.Text = _property.TotalUnits.ToString();
                DpCreatedDate.SelectedDate = _property.CreatedDate;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                if (_property == null)
                    _property = new Property();

                _property.Name = TxtName.Text.Trim();
                _property.Address = string.IsNullOrWhiteSpace(TxtAddress.Text) ? null : TxtAddress.Text.Trim();
                _property.City = string.IsNullOrWhiteSpace(TxtCity.Text) ? null : TxtCity.Text.Trim();
                _property.State = string.IsNullOrWhiteSpace(TxtState.Text) ? null : TxtState.Text.Trim();
                _property.ZipCode = string.IsNullOrWhiteSpace(TxtZipCode.Text) ? null : TxtZipCode.Text.Trim();
                _property.Country = string.IsNullOrWhiteSpace(TxtCountry.Text) ? null : TxtCountry.Text.Trim();
                _property.ManagerName = string.IsNullOrWhiteSpace(TxtManagerName.Text) ? null : TxtManagerName.Text.Trim();
                _property.ContactInfo = string.IsNullOrWhiteSpace(TxtContactInfo.Text) ? null : TxtContactInfo.Text.Trim();
                _property.TotalUnits = int.TryParse(TxtTotalUnits.Text, out int totalUnits) ? totalUnits : 0;
                _property.CreatedDate = DpCreatedDate.SelectedDate ?? DateTime.Now;

                if (_isEditMode)
                {
                    _propertyRepository.Update(_property);
                    MessageBox.Show("Property updated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _propertyRepository.Add(_property);

                    // Automatically create units if specified
                    if (_property.TotalUnits > 0)
                    {
                        CreateUnitsForProperty(_property);
                    }

                    MessageBox.Show("Property added successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving property: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateUnitsForProperty(Property property)
        {
            try
            {
                var unitRepository = new UnitRepository();

                for (int i = 1; i <= property.TotalUnits; i++)
                {
                    var unit = new Unit
                    {
                        PropertyId = property.Id,
                        UnitNumber = $"{i:D3}", // Creates unit numbers like 001, 002, etc.
                        UnitType = "Standard",
                        Size = 0,
                        RentAmount = 0,
                        Bedrooms = 1,
                        Bathrooms = 1,
                        Description = $"Unit {i:D3} in {property.Name}",
                        Status = UnitStatus.Vacant,
                        CreatedDate = DateTime.Now
                    };

                    unitRepository.Add(unit);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating units: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Property name is required.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return false;
            }

            if (!int.TryParse(TxtTotalUnits.Text, out _))
            {
                MessageBox.Show("Please enter a valid number of units.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTotalUnits.Focus();
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
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
    }
}