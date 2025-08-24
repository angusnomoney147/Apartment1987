using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ApartmentManagementSystem
{
    public partial class AddEditPropertyWindow : Window
    {
        private Property? _property;
        private readonly PropertyRepository _propertyRepository;
        private readonly UnitRepository _unitRepository;
        private readonly bool _isEditMode;

        public AddEditPropertyWindow()
        {
            InitializeComponent();
            _propertyRepository = new PropertyRepository();
            _unitRepository = new UnitRepository();
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
            // Set default values
            DpCreatedDate.SelectedDate = DateTime.Now;
        }

        private void LoadPropertyData()
        {
            if (_property != null)
            {
                TxtName.Text = _property.Name;
                TxtAddress.Text = _property.Address;
                TxtCity.Text = _property.City;
                TxtCountry.Text = _property.Country;
                TxtManagerName.Text = _property.ManagerName;
                TxtContactInfo.Text = _property.ContactInfo;
                TxtTotalUnits.Text = _property.TotalUnits.ToString();
                DpCreatedDate.SelectedDate = _property.CreatedDate;
            }
        }

        private void CreateUnitsForProperty(Property property, int numberOfUnits)
        {
            try
            {
                var unitRepository = new UnitRepository();

                for (int i = 1; i <= numberOfUnits; i++)
                {
                    var unit = new Unit
                    {
                        PropertyId = property.Id,
                        UnitNumber = $"{i:D3}", // Creates unit numbers like 001, 002, etc.
                        UnitType = "Standard", // Default unit type
                        Size = 0, // You can set default size
                        RentAmount = 0, // You can set default rent
                        Bedrooms = 1, // Default bedrooms
                        Bathrooms = 1, // Default bathrooms
                        Description = $"Unit {i:D3}",
                        Status = UnitStatus.Vacant, // Default to vacant
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
                    MessageBox.Show("Property added successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    // Automatically create units if specified
                    if (int.TryParse(TxtNumberOfUnits.Text, out int numberOfUnits) && numberOfUnits > 0)
                    {
                        CreateUnitsForProperty(_property, numberOfUnits);
                    }
                }

                MessageBox.Show("Property added successfully! Units have been automatically created.", "Success",
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

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                MessageBox.Show("Property name is required.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return false;
            }

            if (!int.TryParse(TxtTotalUnits.Text, out int totalUnits) || totalUnits < 0)
            {
                MessageBox.Show("Please enter a valid number of units.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTotalUnits.Focus();
                return false;
            }

            return true;
        }

        private void CreateUnitsForProperty(Property property)
        {
            try
            {
                for (int i = 1; i <= property.TotalUnits; i++)
                {
                    var unit = new Unit
                    {
                        PropertyId = property.Id,
                        UnitNumber = $"{property.Name.Substring(0, Math.Min(3, property.Name.Length))}{i:D3}", // PROP001, PROP002, etc.
                        UnitType = "Standard",
                        Size = 0,
                        RentAmount = 0,
                        Description = $"Unit {i} in {property.Name}",
                        Status = UnitStatus.Vacant,
                        Bedrooms = 1,
                        Bathrooms = 1
                    };

                    _unitRepository.Add(unit);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating units for property: {ex.Message}", "Warning",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers
            e.Handled = !IsIntegerAllowed(e.Text);
        }

        private static bool IsIntegerAllowed(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }
    }
}