using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace ApartmentManagementSystem
{
    public partial class LeaseDocumentsWindow : Window
    {
        private readonly LeaseDocumentRepository _documentRepository;
        private readonly Lease _lease;
        private readonly Unit _unit;
        private readonly Tenant _tenant;
        private List<LeaseDocument> _documents;

        public LeaseDocumentsWindow(Lease lease, Unit unit, Tenant tenant)
        {
            InitializeComponent();
            _documentRepository = new LeaseDocumentRepository();
            _lease = lease;
            _unit = unit;
            _tenant = tenant;

            TxtLeaseInfo.Text = $"Lease: Unit {_unit?.UnitNumber ?? "Unknown"} - {_tenant?.FullName ?? "Unknown Tenant"}";

            // If this is a new lease (ID = 0), don't load documents yet
            if (_lease.Id > 0)
            {
                LoadDocuments();
            }
            else
            {
                // For new leases, show a message
                DataGridDocuments.ItemsSource = new[] { new { Message = "Documents will be available after lease is saved." } };
                DataGridDocuments.Columns.Clear();
                DataGridDocuments.AutoGenerateColumns = true;
            }
        }

        private void LoadDocuments()
        {
            try
            {
                _documents = _documentRepository.GetDocumentsByLease(_lease.Id);

                // Add type names for display
                var documentsWithTypes = _documents.Select(d => new
                {
                    d.Id,
                    d.DocumentName,
                    Type = GetDocumentTypeName(d.Type),
                    d.UploadDate,
                    d.Notes
                }).ToList();

                DataGridDocuments.ItemsSource = documentsWithTypes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading documents: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void BtnAddDocument_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|PDF Files (*.pdf)|*.pdf|Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Text Files (*.txt)|*.txt",
                Title = "Select Document to Upload"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var addDocumentWindow = new AddDocumentWindow();
                if (addDocumentWindow.ShowDialog() == true)
                {
                    try
                    {
                        // Copy file to documents directory
                        var documentsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documents");
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(openFileDialog.FileName)}";
                        var destinationPath = Path.Combine(documentsDir, fileName);

                        File.Copy(openFileDialog.FileName, destinationPath);

                        var document = new LeaseDocument
                        {
                            LeaseId = _lease.Id,
                            DocumentName = addDocumentWindow.DocumentName,
                            FilePath = destinationPath,
                            Type = addDocumentWindow.SelectedType,
                            UploadDate = DateTime.Now,
                            Notes = addDocumentWindow.Notes
                        };

                        _documentRepository.AddDocument(document);
                        LoadDocuments();

                        MessageBox.Show("Document added successfully!", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding document: {ex.Message}", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnViewDocument_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDocuments.SelectedItem != null)
            {
                // Get the actual document ID
                var selectedItem = DataGridDocuments.SelectedItem;
                var documentIdProperty = selectedItem.GetType().GetProperty("Id");
                int documentId = documentIdProperty != null ? (int)documentIdProperty.GetValue(selectedItem) : 0;

                if (documentId > 0)
                {
                    var document = _documents.FirstOrDefault(d => d.Id == documentId);
                    if (document != null && File.Exists(document.FilePath))
                    {
                        try
                        {
                            // Check if it's a PDF file
                            if (document.FilePath.ToLower().EndsWith(".pdf"))
                            {
                                // Open PDF in default PDF viewer
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = document.FilePath,
                                    UseShellExecute = true
                                });
                            }
                            else
                            {
                                // For other files, ask user what to do
                                var result = MessageBox.Show($"This is a {Path.GetExtension(document.FilePath)} file. Would you like to open it with the default application?",
                                                           "Open Document",
                                                           MessageBoxButton.YesNo,
                                                           MessageBoxImage.Question);

                                if (result == MessageBoxResult.Yes)
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = document.FilePath,
                                        UseShellExecute = true
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error opening document: {ex.Message}", "Error",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Document file not found.", "Error",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a document to view.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridDocuments.SelectedItem != null)
            {
                // Get the actual document ID
                var selectedItem = DataGridDocuments.SelectedItem;
                var documentIdProperty = selectedItem.GetType().GetProperty("Id");
                int documentId = documentIdProperty != null ? (int)documentIdProperty.GetValue(selectedItem) : 0;

                if (documentId > 0)
                {
                    var result = MessageBox.Show("Are you sure you want to delete this document?",
                                           "Confirm Delete",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            _documentRepository.DeleteDocument(documentId);
                            LoadDocuments();
                            MessageBox.Show("Document deleted successfully!", "Success",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting document: {ex.Message}", "Error",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a document to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}