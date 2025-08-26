using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class PaymentManagementWindow : Window
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly LeaseRepository _leaseRepository;
        private readonly UnitRepository _unitRepository;
        private readonly TenantRepository _tenantRepository;
        private readonly PropertyRepository _propertyRepository;
        private List<Payment> _allPayments = new();
        private List<Lease> _allLeases = new();
        private List<Unit> _allUnits = new();
        private List<Tenant> _allTenants = new();
        private List<Property> _allProperties = new();

        public PaymentManagementWindow()
        {
            InitializeComponent();
            _paymentRepository = new PaymentRepository();
            _leaseRepository = new LeaseRepository();
            _unitRepository = new UnitRepository();
            _tenantRepository = new TenantRepository();
            _propertyRepository = new PropertyRepository();
            InitializeFilters();
            LoadAllData();
        }

        private void InitializeFilters()
        {
            // Populate status filter dropdown
            var statuses = Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>()
                .Select(s => new { Value = (int)s, Name = PaymentStatusHelper.GetStatusName(s) })
                .ToList();

            CmbStatusFilter.ItemsSource = statuses;
            CmbStatusFilter.DisplayMemberPath = "Name";
            CmbStatusFilter.SelectedValuePath = "Value";

            // Populate method filter dropdown
            var methods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                .Select(m => new { Value = (int)m, Name = PaymentMethodHelper.GetMethodName(m) })
                .ToList();

            CmbMethodFilter.ItemsSource = methods;
            CmbMethodFilter.DisplayMemberPath = "Name";
            CmbMethodFilter.SelectedValuePath = "Value";
        }

        private void LoadAllData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading all data...");
                _allPayments = _paymentRepository.GetAll();
                _allLeases = _leaseRepository.GetAll();
                _allUnits = _unitRepository.GetAll();
                _allTenants = _tenantRepository.GetAll();
                _allProperties = _propertyRepository.GetAll();

                System.Diagnostics.Debug.WriteLine($"Loaded {_allPayments.Count} payments");
                RefreshPaymentGrid();
                System.Diagnostics.Debug.WriteLine("Data grid refreshed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading  {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridPayments.SelectedItem != null)
            {
                try
                {
                    // Get the selected payment ID
                    var selectedItem = DataGridPayments.SelectedItem;
                    var paymentIdProperty = selectedItem.GetType().GetProperty("Id");
                    int selectedPaymentId = paymentIdProperty != null ? (int)paymentIdProperty.GetValue(selectedItem) : 0;

                    if (selectedPaymentId > 0)
                    {
                        // Get the full payment object with all details
                        var paymentRepository = new PaymentRepository();
                        var payment = paymentRepository.GetById(selectedPaymentId);

                        if (payment != null)
                        {
                            // Get related data
                            var leaseRepository = new LeaseRepository();
                            var unitRepository = new UnitRepository();
                            var tenantRepository = new TenantRepository();
                            var propertyRepository = new PropertyRepository();

                            var lease = leaseRepository.GetById(payment.LeaseId);
                            var unit = lease != null ? unitRepository.GetById(lease.UnitId) : null;
                            var tenant = lease != null ? tenantRepository.GetById(lease.TenantId) : null;
                            var property = unit != null ? propertyRepository.GetById(unit.PropertyId) : null;

                            // Generate professional invoice
                            GenerateProfessionalInvoice(payment, lease, unit, tenant, property);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating invoice: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a payment to generate invoice.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GenerateProfessionalInvoice(Payment payment, Lease lease, Unit unit, Tenant tenant, Property property)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf|Word Files (*.docx)|*.docx",
                    FileName = $"Invoice_{payment.Id}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    if (saveDialog.FileName.EndsWith(".pdf"))
                    {
                        GeneratePdfInvoice(saveDialog.FileName, payment, lease, unit, tenant, property);
                        MessageBox.Show("Professional invoice generated successfully!", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        GenerateWordInvoice(saveDialog.FileName, payment, lease, unit, tenant, property);
                        MessageBox.Show("Professional invoice generated successfully!", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating invoice: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GeneratePdfInvoice(string fileName, Payment payment, Lease lease, Unit unit, Tenant tenant, Property property)
        {
            try
            {
                var document = new Document(PageSize.A4, 50, 50, 25, 25);
                var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                document.Open();

                // Professional Receipt Template
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

                // Company Header
                var companyHeader = new Paragraph("LEASE PAYMENT RECEIPT", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };
                document.Add(companyHeader);

                // Receipt Details Table
                var detailsTable = new PdfPTable(2) { WidthPercentage = 100 };
                detailsTable.SetWidths(new float[] { 1f, 1f });

                // Left side - From
                var leftCell = new PdfPCell();
                leftCell.Border = PdfPCell.NO_BORDER;
                var leftContent = new Paragraph();
                leftContent.Add(new Chunk("FROM:\n", headerFont));
                leftContent.Add(new Chunk("Apartment Management System\n", normalFont));
                leftContent.Add(new Chunk("123 Property Management St.\n", normalFont));
                leftContent.Add(new Chunk("Business City, BC 12345\n", normalFont));
                leftContent.Add(new Chunk("Phone: (555) 123-4567\n", normalFont));
                leftContent.Add(new Chunk("Email: info@apartmentmgmt.com\n", normalFont));
                leftCell.AddElement(leftContent);
                detailsTable.AddCell(leftCell);

                // Right side - To & Receipt Info
                var rightCell = new PdfPCell();
                rightCell.Border = PdfPCell.NO_BORDER;

                var rightContent = new Paragraph();
                rightContent.Add(new Chunk("TO:\n", headerFont));
                rightContent.Add(new Chunk($"{tenant?.FullName ?? "Unknown Tenant"}\n", normalFont));
                rightContent.Add(new Chunk($"{tenant?.Address ?? "Unknown Address"}\n", normalFont));
                rightContent.Add(new Chunk($"Phone: {tenant?.Phone ?? "N/A"}\n", normalFont));
                rightContent.Add(new Chunk($"Email: {tenant?.Email ?? "N/A"}\n\n", normalFont));

                rightContent.Add(new Chunk("RECEIPT #:", headerFont));
                rightContent.Add(new Chunk($" RCT-{payment.Id:D6}\n", normalFont));
                rightContent.Add(new Chunk("DATE:", headerFont));
                rightContent.Add(new Chunk($" {payment.PaymentDate:MM/dd/yyyy}\n", normalFont));
                rightContent.Add(new Chunk("FOR PERIOD:", headerFont));
                rightContent.Add(new Chunk($" {payment.PaymentDate:MMMM yyyy}\n", normalFont)); // Monthly period

                rightCell.AddElement(rightContent);
                detailsTable.AddCell(rightCell);

                document.Add(detailsTable);
                document.Add(new Paragraph(" ") { SpacingAfter = 20 });

                // Items Table
                var itemsTable = new PdfPTable(5) { WidthPercentage = 100 };
                itemsTable.SetWidths(new float[] { 3f, 1f, 1f, 1f, 1f });

                // Table Headers
                itemsTable.AddCell(CreatePdfCell("DESCRIPTION", headerFont, BaseColor.LIGHT_GRAY));
                itemsTable.AddCell(CreatePdfCell("QTY", headerFont, BaseColor.LIGHT_GRAY));
                itemsTable.AddCell(CreatePdfCell("UNIT PRICE", headerFont, BaseColor.LIGHT_GRAY));
                itemsTable.AddCell(CreatePdfCell("TAX", headerFont, BaseColor.LIGHT_GRAY));
                itemsTable.AddCell(CreatePdfCell("TOTAL", headerFont, BaseColor.LIGHT_GRAY));

                // Item Row - Monthly Payment
                itemsTable.AddCell(CreatePdfCell($"Monthly Lease Payment - {property?.Name ?? "Unknown Property"}, Unit {unit?.UnitNumber ?? "Unknown Unit"}", normalFont));
                itemsTable.AddCell(CreatePdfCell("1", normalFont));
                itemsTable.AddCell(CreatePdfCell($"${payment.Amount:F2}", normalFont));
                itemsTable.AddCell(CreatePdfCell("$0.00", normalFont));
                itemsTable.AddCell(CreatePdfCell($"${payment.Amount:F2}", normalFont));

                // Empty rows for spacing
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));

                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("SUBTOTAL", headerFont));
                itemsTable.AddCell(CreatePdfCell($"${payment.Amount:F2}", headerFont));

                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("TAX", headerFont));
                itemsTable.AddCell(CreatePdfCell("$0.00", headerFont));

                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("", normalFont));
                itemsTable.AddCell(CreatePdfCell("TOTAL PAID", headerFont));
                itemsTable.AddCell(CreatePdfCell($"${payment.Amount:F2}", headerFont));

                document.Add(itemsTable);

                // Payment Method
                document.Add(new Paragraph(" ") { SpacingAfter = 20 });
                var paymentMethod = new Paragraph($"PAYMENT METHOD: {PaymentMethodHelper.GetMethodName(payment.Method)}", normalFont)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(paymentMethod);

                // Payment Status
                document.Add(new Paragraph(" ") { SpacingAfter = 20 });
                var paymentStatus = new Paragraph($"PAYMENT STATUS: {PaymentStatusHelper.GetStatusName(payment.Status)}", normalFont)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(paymentStatus);

                // Notes
                document.Add(new Paragraph(" ") { SpacingAfter = 20 });
                var notes = new Paragraph("NOTES", headerFont);
                document.Add(notes);
                var notesContent = new Paragraph("This receipt confirms receipt of monthly lease payment. Please retain this receipt for your records.", normalFont);
                document.Add(notesContent);

                // Footer
                document.Add(new Paragraph(" ") { SpacingBefore = 30 });
                var separator = new Paragraph(new string('-', 80), smallFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(separator);

                var footer = new Paragraph("THANK YOU FOR YOUR PAYMENT!", headerFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                document.Add(footer);

                var companyInfo = new Paragraph("Apartment Management System • 123 Property Management St. • Business City, BC 12345", smallFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 5
                };
                document.Add(companyInfo);

                var contactInfo = new Paragraph("Phone: (555) 123-4567 • Email: info@apartmentmgmt.com • Website: www.apartmentmgmt.com", smallFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(contactInfo);

                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF receipt: {ex.Message}", ex);
            }
        }
        private void GenerateWordInvoice(string fileName, Payment payment, Lease lease, Unit unit, Tenant tenant, Property property)
        {
            try
            {
                // Simple text version for Word
                var content = $@"LEASE PAYMENT RECEIPT

FROM:
Apartment Management System
123 Property Management St.
Business City, BC 12345
Phone: (555) 123-4567
Email: info@apartmentmgmt.com

TO:
{tenant?.FullName ?? "Unknown Tenant"}
{tenant?.Address ?? "Unknown Address"}
Phone: {tenant?.Phone ?? "N/A"}
Email: {tenant?.Email ?? "N/A"}

RECEIPT #: RCT-{payment.Id:D6}
DATE: {payment.PaymentDate:MM/dd/yyyy}
FOR PERIOD: {payment.PaymentDate:MMMM yyyy}

-------------------------------------------------------------------------------

DESCRIPTION                                    QTY    UNIT PRICE    TAX      TOTAL
Monthly Lease Payment - {property?.Name ?? "Unknown Property"}, Unit {unit?.UnitNumber ?? "Unknown Unit"}    1      ${payment.Amount:F2}       $0.00    ${payment.Amount:F2}

                                               SUBTOTAL:           ${payment.Amount:F2}
                                               TAX:                $0.00
                                               TOTAL PAID:         ${payment.Amount:F2}

PAYMENT METHOD: {PaymentMethodHelper.GetMethodName(payment.Method)}
PAYMENT STATUS: {PaymentStatusHelper.GetStatusName(payment.Status)}

NOTES:
This receipt confirms receipt of monthly lease payment. Please retain this receipt for your records.

-------------------------------------------------------------------------------

THANK YOU FOR YOUR PAYMENT!

Apartment Management System
123 Property Management St.
Business City, BC 12345
Phone: (555) 123-4567
Email: info@apartmentmgmt.com
Website: www.apartmentmgmt.com";

                System.IO.File.WriteAllText(fileName, content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating Word receipt: {ex.Message}", ex);
            }
        }
        private PdfPCell CreatePdfCell(string text, Font font, BaseColor backgroundColor = null)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 8,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }

            return cell;
        }

        private void RefreshPaymentGrid()
        {
            try
            {
                var paymentsWithInfo = new List<object>();

                foreach (var p in _allPayments)
                {
                    try
                    {
                        var paymentInfo = new
                        {
                            p.Id,
                            LeaseInfo = GetLeaseInfo(p.LeaseId),
                            p.Amount,
                            p.PaymentDate,
                            Method = PaymentMethodHelper.GetMethodName(p.Method),
                            p.ReferenceNumber,
                            Status = PaymentStatusHelper.GetStatusName(p.Status),
                            p.Notes
                        };
                        paymentsWithInfo.Add(paymentInfo);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing payment {p.Id}: {ex.Message}");
                    }
                }

                DataGridPayments.ItemsSource = paymentsWithInfo;
                System.Diagnostics.Debug.WriteLine($"Refreshed {paymentsWithInfo.Count} payments in grid");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RefreshPaymentGrid: {ex.Message}");
            }
        }

        private string GetLeaseInfo(int leaseId)
        {
            var lease = _allLeases.FirstOrDefault(l => l.Id == leaseId);
            if (lease != null)
            {
                var unit = _allUnits.FirstOrDefault(u => u.Id == lease.UnitId);
                var tenant = _allTenants.FirstOrDefault(t => t.Id == lease.TenantId);
                var property = unit != null ? _allProperties.FirstOrDefault(p => p.Id == unit.PropertyId) : null;

                var unitNumber = unit?.UnitNumber ?? "Unknown Unit";
                var tenantName = tenant?.FullName ?? "Unknown Tenant";
                var propertyName = property?.Name ?? "Unknown Property";

                return $"{propertyName}, {unitNumber}, {tenantName}";
            }
            return $"Lease {leaseId}";
        }

        private void NotifyParentOfChanges()
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.ManualRefresh();
            MainWindow.NotifyDataChanged();
        }

        private void BtnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            var addPaymentWindow = new AddEditPaymentWindow(_allLeases, _allUnits, _allTenants, _allProperties);
            if (addPaymentWindow.ShowDialog() == true)
            {
                LoadAllData();
                NotifyParentOfChanges();
                RefreshPaymentGrid();

                MessageBox.Show("Payment list refreshed!", "Info",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditPayment_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridPayments.SelectedItem != null)
            {
                // Get the actual payment ID from the selected row
                var selectedItem = DataGridPayments.SelectedItem;
                var paymentIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedPaymentId = paymentIdProperty != null ? (int)paymentIdProperty.GetValue(selectedItem) : 0;

                if (selectedPaymentId > 0)
                {
                    var paymentToEdit = _allPayments.FirstOrDefault(p => p.Id == selectedPaymentId);
                    if (paymentToEdit != null)
                    {
                        var editPaymentWindow = new AddEditPaymentWindow(paymentToEdit, _allLeases, _allUnits, _allTenants, _allProperties);
                        if (editPaymentWindow.ShowDialog() == true)
                        {
                            LoadAllData(); // Refresh the list
                            NotifyParentOfChanges(); // Add this line
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a payment to edit.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridPayments.SelectedItem != null)
            {
                // Get the actual payment ID from the selected row
                var selectedItem = DataGridPayments.SelectedItem;
                var paymentIdProperty = selectedItem.GetType().GetProperty("Id");
                int selectedPaymentId = paymentIdProperty != null ? (int)paymentIdProperty.GetValue(selectedItem) : 0;

                if (selectedPaymentId > 0)
                {
                    var paymentToDelete = _allPayments.FirstOrDefault(p => p.Id == selectedPaymentId);
                    if (paymentToDelete != null)
                    {
                        var leaseInfo = GetLeaseInfo(paymentToDelete.LeaseId);

                        var result = MessageBox.Show($"Are you sure you want to delete the payment for {leaseInfo}?",
                                                   "Confirm Delete",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _paymentRepository.Delete(paymentToDelete.Id);
                                LoadAllData(); // Refresh the list
                                NotifyParentOfChanges(); // Add this line
                                MessageBox.Show("Payment deleted successfully!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting payment: {ex.Message}", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a payment to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbStatusFilter.SelectedItem != null)
            {
                var selectedStatus = (int)CmbStatusFilter.SelectedValue;
                var filteredPayments = _allPayments.Where(p => (int)p.Status == selectedStatus).ToList();
                RefreshFilteredPaymentGrid(filteredPayments);
            }
        }

        private void CmbMethodFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMethodFilter.SelectedItem != null)
            {
                var selectedMethod = (int)CmbMethodFilter.SelectedValue;
                var filteredPayments = _allPayments.Where(p => (int)p.Method == selectedMethod).ToList();
                RefreshFilteredPaymentGrid(filteredPayments);
            }
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            CmbStatusFilter.SelectedIndex = -1;
            CmbMethodFilter.SelectedIndex = -1;
            RefreshPaymentGrid();
        }

        private void BtnPaidOnly_Click(object sender, RoutedEventArgs e)
        {
            var paidPayments = _allPayments.Where(p => p.Status == PaymentStatus.Completed).ToList();
            RefreshFilteredPaymentGrid(paidPayments);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                RefreshPaymentGrid();
            }
            else
            {
                var filteredPayments = _allPayments.Where(p =>
                    GetLeaseInfo(p.LeaseId).ToLower().Contains(searchText) ||
                    p.ReferenceNumber?.ToLower().Contains(searchText) == true ||
                    p.Notes?.ToLower().Contains(searchText) == true).ToList();

                RefreshFilteredPaymentGrid(filteredPayments);
            }
        }

        private void RefreshFilteredPaymentGrid(List<Payment> payments)
        {
            var paymentsWithInfo = payments.Select(p => new
            {
                p.Id,
                LeaseInfo = GetLeaseInfo(p.LeaseId),
                p.Amount,
                p.PaymentDate,
                Method = PaymentMethodHelper.GetMethodName(p.Method),
                p.ReferenceNumber,
                Status = PaymentStatusHelper.GetStatusName(p.Status),
                p.Notes
            }).ToList();

            DataGridPayments.ItemsSource = paymentsWithInfo;
        }
    }
}