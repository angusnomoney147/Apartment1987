using System;
using System.Linq;
using System.Windows;

namespace ApartmentManagementSystem
{
    public partial class AddDocumentWindow : Window
    {
        public string DocumentName { get; private set; }
        public DocumentType SelectedType { get; private set; }
        public string Notes { get; private set; }

        public AddDocumentWindow()
        {
            InitializeComponent();
            InitializeDocumentTypes();
        }

        private void InitializeDocumentTypes()
        {
            var documentTypes = Enum.GetValues(typeof(DocumentType))
                .Cast<DocumentType>()
                .Select(dt => new { Value = (int)dt, Name = GetDocumentTypeName(dt) })
                .ToList();

            CmbDocumentType.ItemsSource = documentTypes;
            CmbDocumentType.DisplayMemberPath = "Name";
            CmbDocumentType.SelectedValuePath = "Value";
            CmbDocumentType.SelectedIndex = 0; // Default to Lease Agreement
        }

        private string GetDocumentTypeName(DocumentType type)
        {
            return type switch
            {
                DocumentType.LeaseAgreement => "Lease Agreement",
                DocumentType.IDScan => "ID Scan",
                DocumentType.Passport => "Passport",
                DocumentType.DriverLicense => "Driver License",
                DocumentType.Other => "Other Document",
                _ => "Unknown"
            };
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtDocumentName.Text))
            {
                MessageBox.Show("Please enter a document name.", "Validation Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDocumentName.Focus();
                return;
            }

            DocumentName = TxtDocumentName.Text.Trim();
            SelectedType = (DocumentType)(int)CmbDocumentType.SelectedValue;
            Notes = TxtNotes.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}