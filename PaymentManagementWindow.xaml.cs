using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections;
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
            var statuses = Enum.GetValues(typeof(PaymentStatus)).Cast<PaymentStatus>()
                .Select(s => new { Value = (int)s, Name = PaymentStatusHelper.GetStatusName(s) })
                .ToList();
            CmbStatusFilter.ItemsSource = statuses;
            CmbStatusFilter.DisplayMemberPath = "Name";
            CmbStatusFilter.SelectedValuePath = "Value";

            var methods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>()
                .Select(m => new { Value = (int)m, Name = PaymentMethodHelper.GetMethodName(m) })
                .ToList();
            CmbMethodFilter.ItemsSource = methods;
            CmbMethodFilter.DisplayMemberPath = "Name";
            CmbMethodFilter.SelectedValuePath = "Value";
        }

        private void InitializeTenantFilter()
        {
            try
            {
                _allTenants = _tenantRepository.GetAll();
                _allLeases = _leaseRepository.GetAll();
                var tenantsWithAll = new List<object> { new { Id = 0, Name = "All Tenants" } };
                tenantsWithAll.AddRange(_allTenants.Where(t => t.IsActive)
                                                  .Select(t => new { t.Id, Name = t.FullName })
                                                  .ToList());
                CmbTenantFilter.ItemsSource = tenantsWithAll;
                CmbTenantFilter.DisplayMemberPath = "Name";
                CmbTenantFilter.SelectedValuePath = "Id";
                CmbTenantFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing tenant filter: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummaryDashboard()
        {
            decimal totalPaid = _allPayments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
            decimal totalOverdue = _allPayments.Where(p => p.Status == PaymentStatus.Overdue).Sum(p => p.Amount);
            decimal totalPending = _allPayments.Where(p => p.Status == PaymentStatus.Pending).Sum(p => p.Amount);
            decimal totalCancelled = _allPayments.Where(p => p.Status == PaymentStatus.Cancelled).Sum(p => p.Amount);

            TxtTotalPaid.Text = totalPaid.ToString("C");
            TxtTotalOverdue.Text = totalOverdue.ToString("C");
            TxtTotalPending.Text = totalPending.ToString("C");
            TxtTotalCancelled.Text = totalCancelled.ToString("C");
        }

        private void LoadTenantPayments(int tenantId = 0)
        {
            try
            {
                var payments = _paymentRepository.GetAll();
                if (tenantId > 0)
                {
                    var tenantLeases = _allLeases.Where(l => l.TenantId == tenantId)
                                                .Select(l => l.Id)
                                                .ToHashSet();
                    payments = payments.Where(p => tenantLeases.Contains(p.LeaseId)).ToList();
                }

                var paymentsWithInfo = payments.Select(p =>
                {
                    var lease = _allLeases.FirstOrDefault(l => l.Id == p.LeaseId);
                    var unit = lease != null ? _allUnits.FirstOrDefault(u => u.Id == lease.UnitId) : null;
                    var tenant = lease != null ? _allTenants.FirstOrDefault(t => t.Id == lease.TenantId) : null;
                    var property = unit != null ? _allProperties.FirstOrDefault(prop => prop.Id == unit.PropertyId) : null;

                    return new
                    {
                        p.Id,
                        LeaseInfo = $"{property?.Name ?? "Unknown Property"}, Unit {unit?.UnitNumber ?? "Unknown Unit"}, {tenant?.FullName ?? "Unknown Tenant"}",
                        p.Amount,
                        p.PaymentDate,
                        Method = PaymentMethodHelper.GetMethodName(p.Method),
                        Status = PaymentStatusHelper.GetStatusName(p.Status),
                        p.ReferenceNumber,
                        p.Notes,
                        OriginalStatus = p.Status
                    };
                })
                .OrderBy(p => p.Id) // CHANGE: Replaced date sorting with default sorting by ID
                .ToList();

                DataGridTenantPayments.ItemsSource = paymentsWithInfo;
            }
            catch (Exception ex)
            {
                DataGridTenantPayments.ItemsSource = new List<object>();
                MessageBox.Show($"Error loading tenant payments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbTenantFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTenantFilter.SelectedValue != null)
            {
                int tenantId = (int)CmbTenantFilter.SelectedValue;
                LoadTenantPayments(tenantId);
            }
        }

        private void BtnClearTenantFilter_Click(object sender, RoutedEventArgs e)
        {
            CmbTenantFilter.SelectedIndex = 0;
            LoadTenantPayments();
        }

        private void LoadAllData()
        {
            try
            {
                _allPayments = _paymentRepository.GetAll();
                _allLeases = _leaseRepository.GetAll();
                _allUnits = _unitRepository.GetAll();
                _allTenants = _tenantRepository.GetAll();
                _allProperties = _propertyRepository.GetAll();
                RefreshPaymentGrid();
                InitializeTenantFilter();
                LoadTenantPayments();
                UpdateSummaryDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerateInvoice_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = this.TabControl.SelectedItem as TabItem;
            if (selectedTab?.Header.ToString() == "By Tenant")
            {
                if (DataGridTenantPayments.SelectedItem != null)
                {
                    GenerateInvoiceFromGrid(DataGridTenantPayments.SelectedItem);
                }
                else if (DataGridTenantPayments.Items.Count > 0)
                {
                    GenerateBulkInvoiceFromGrid(DataGridTenantPayments.ItemsSource);
                }
                else
                {
                    MessageBox.Show("Please select a payment or ensure there are payments to generate invoice.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (DataGridPayments.SelectedItem != null)
                {
                    GenerateInvoiceFromGrid(DataGridPayments.SelectedItem);
                }
                else if (DataGridPayments.Items.Count > 0)
                {
                    GenerateBulkInvoiceFromGrid(DataGridPayments.ItemsSource);
                }
                else
                {
                    MessageBox.Show("Please select a payment or ensure there are payments to generate invoice.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnGenerateTenantReport_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTenantFilter.SelectedValue == null || (int)CmbTenantFilter.SelectedValue == 0)
            {
                MessageBox.Show("Please select a specific tenant from the dropdown list to generate a report.", "Select Tenant", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (DataGridTenantPayments.Items.Count == 0)
            {
                MessageBox.Show("There are no payments to export for the selected tenant.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var selectedTenantItem = CmbTenantFilter.SelectedItem;
                var tenantName = selectedTenantItem.GetType().GetProperty("Name")?.GetValue(selectedTenantItem, null)?.ToString() ?? "Unknown_Tenant";
                string safeTenantName = string.Join("_", tenantName.Split(Path.GetInvalidFileNameChars()));

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Payment_Report_{safeTenantName}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var itemsList = ((IEnumerable)DataGridTenantPayments.ItemsSource).Cast<object>().ToList();
                    string reportTitle = $"Payment Report for {tenantName}";
                    GenerateBulkPdfInvoice(itemsList, saveDialog.FileName, reportTitle);
                    MessageBox.Show($"Payment report for {tenantName} generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the tenant report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateBulkPdfInvoice(List<object> itemsList, string fileName, string reportTitle = "PAYMENT REPORT")
        {
            try
            {
                var document = new Document(PageSize.A4, 36, 36, 54, 36);
                var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                writer.PageEvent = new PdfPageEvents(reportTitle);
                document.Open();

                var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                var titleFont = new Font(baseFont, 20, Font.BOLD, new BaseColor(34, 49, 63));
                var headerFont = new Font(baseFont, 11, Font.BOLD, new BaseColor(52, 73, 94));
                var normalFont = new Font(baseFont, 9, Font.NORMAL, BaseColor.BLACK);
                var whiteBoldFont = new Font(baseFont, 10, Font.BOLD, BaseColor.WHITE);
                var tableHeaderColor = new BaseColor(65, 84, 103);
                var alternateRowColor = new BaseColor(245, 245, 245);

                string subTitleText = (itemsList.Count == 1) ? "Payment Receipt Details" : $"Covering {itemsList.Count} payment records";

                var docTitle = new Paragraph(reportTitle, titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 5 };
                var subTitle = new Paragraph(subTitleText, new Font(baseFont, 12, Font.ITALIC, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(docTitle);
                document.Add(subTitle);

                document.Add(new Paragraph("FINANCIAL OVERVIEW", headerFont) { SpacingAfter = 10 });

                decimal totalCompleted = itemsList.Where(p => (PaymentStatus)p.GetType().GetProperty("OriginalStatus").GetValue(p) == PaymentStatus.Completed).Sum(p => (decimal)p.GetType().GetProperty("Amount").GetValue(p));
                decimal totalOverdue = itemsList.Where(p => (PaymentStatus)p.GetType().GetProperty("OriginalStatus").GetValue(p) == PaymentStatus.Overdue).Sum(p => (decimal)p.GetType().GetProperty("Amount").GetValue(p));
                decimal totalPending = itemsList.Where(p => (PaymentStatus)p.GetType().GetProperty("OriginalStatus").GetValue(p) == PaymentStatus.Pending).Sum(p => (decimal)p.GetType().GetProperty("Amount").GetValue(p));
                decimal totalCancelled = itemsList.Where(p => (PaymentStatus)p.GetType().GetProperty("OriginalStatus").GetValue(p) == PaymentStatus.Cancelled).Sum(p => (decimal)p.GetType().GetProperty("Amount").GetValue(p));
                decimal grandTotal = itemsList.Sum(p => (decimal)p.GetType().GetProperty("Amount").GetValue(p));

                var summaryTable = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 25 };
                summaryTable.SetWidths(new[] { 1f, 1f, 1f, 1f });

                summaryTable.AddCell(CreateSummaryCell("Total Paid (Completed)", totalCompleted.ToString("C"), new BaseColor(39, 174, 96)));
                summaryTable.AddCell(CreateSummaryCell("Total Overdue", totalOverdue.ToString("C"), new BaseColor(231, 76, 60)));
                summaryTable.AddCell(CreateSummaryCell("Total Pending", totalPending.ToString("C"), new BaseColor(241, 196, 15)));
                summaryTable.AddCell(CreateSummaryCell("Total Cancelled", totalCancelled.ToString("C"), new BaseColor(149, 165, 166)));
                document.Add(summaryTable);

                document.Add(new Paragraph("DETAILED PAYMENT RECORDS", headerFont) { SpacingAfter = 10 });

                var detailTable = new PdfPTable(7) { WidthPercentage = 100, HeaderRows = 1 };
                detailTable.SetWidths(new[] { 0.5f, 2.2f, 0.8f, 1f, 0.9f, 0.9f, 1.2f });

                detailTable.AddCell(CreateHeaderCell("ID", whiteBoldFont, tableHeaderColor));
                detailTable.AddCell(CreateHeaderCell("Lease & Tenant Info", whiteBoldFont, tableHeaderColor));
                detailTable.AddCell(CreateHeaderCell("Amount", whiteBoldFont, tableHeaderColor, Element.ALIGN_RIGHT));
                detailTable.AddCell(CreateHeaderCell("Payment Date", whiteBoldFont, tableHeaderColor, Element.ALIGN_CENTER));
                detailTable.AddCell(CreateHeaderCell("Method", whiteBoldFont, tableHeaderColor, Element.ALIGN_CENTER));
                detailTable.AddCell(CreateHeaderCell("Status", whiteBoldFont, tableHeaderColor, Element.ALIGN_CENTER));
                detailTable.AddCell(CreateHeaderCell("Reference #", whiteBoldFont, tableHeaderColor));

                bool alternate = false;
                foreach (var item in itemsList)
                {
                    var itemType = item.GetType();
                    var status = itemType.GetProperty("Status")?.GetValue(item)?.ToString() ?? "";
                    var rowColor = alternate ? alternateRowColor : BaseColor.WHITE;

                    detailTable.AddCell(CreateBodyCell(itemType.GetProperty("Id")?.GetValue(item)?.ToString() ?? "", normalFont, Element.ALIGN_CENTER, rowColor));
                    detailTable.AddCell(CreateBodyCell(itemType.GetProperty("LeaseInfo")?.GetValue(item)?.ToString() ?? "", normalFont, Element.ALIGN_LEFT, rowColor));
                    detailTable.AddCell(CreateBodyCell(((decimal)itemType.GetProperty("Amount")?.GetValue(item)).ToString("C"), normalFont, Element.ALIGN_RIGHT, rowColor));
                    var paymentDate = (itemType.GetProperty("PaymentDate")?.GetValue(item) as DateTime?) ?? DateTime.Now;
                    detailTable.AddCell(CreateBodyCell(paymentDate.ToString("yyyy-MM-dd"), normalFont, Element.ALIGN_CENTER, rowColor));
                    detailTable.AddCell(CreateBodyCell(itemType.GetProperty("Method")?.GetValue(item)?.ToString() ?? "", normalFont, Element.ALIGN_CENTER, rowColor));
                    detailTable.AddCell(CreateBodyCell(status, normalFont, Element.ALIGN_CENTER, rowColor));
                    detailTable.AddCell(CreateBodyCell(itemType.GetProperty("ReferenceNumber")?.GetValue(item)?.ToString() ?? "", normalFont, Element.ALIGN_LEFT, rowColor));

                    alternate = !alternate;
                }

                var totalCell = new PdfPCell(new Phrase("Grand Total", whiteBoldFont))
                {
                    Colspan = 2,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 8,
                    BackgroundColor = tableHeaderColor
                };
                detailTable.AddCell(totalCell);

                var totalAmountCell = new PdfPCell(new Phrase(grandTotal.ToString("C"), whiteBoldFont))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 8,
                    BackgroundColor = tableHeaderColor
                };
                detailTable.AddCell(totalAmountCell);

                var emptyFooterCell = new PdfPCell(new Phrase(" ", whiteBoldFont))
                {
                    Colspan = 4,
                    BackgroundColor = tableHeaderColor,
                    BorderColor = tableHeaderColor
                };
                detailTable.AddCell(emptyFooterCell);
                document.Add(detailTable);

                var footerTable = new PdfPTable(1) { WidthPercentage = 100, SpacingBefore = 30f };
                footerTable.DefaultCell.Border = Rectangle.NO_BORDER;
                var thankYouCell = new PdfPCell
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderColor = new BaseColor(221, 221, 221),
                    Padding = 15f
                };

                var thankYouPara = new Paragraph("THANK YOU FOR YOUR PAYMENT!", new Font(baseFont, 16, Font.BOLD, new BaseColor(52, 152, 219)))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 5f
                };
                thankYouCell.AddElement(thankYouPara);

                var notesPara = new Paragraph("Please retain this receipt for your records. If you have any questions, please contact us.", new Font(baseFont, 8, Font.ITALIC))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                thankYouCell.AddElement(notesPara);
                footerTable.AddCell(thankYouCell);
                document.Add(footerTable);

                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating bulk PDF invoice: {ex.Message}", ex);
            }
        }
        private PdfPCell CreatePdfCell(string text, Font font, BaseColor backgroundColor = null)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 8,
                HorizontalAlignment = Element.ALIGN_LEFT,
            };

            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }

            return cell;
        }

        private PdfPCell CreateHeaderCell(string text, Font font, BaseColor backgroundColor, int horizontalAlignment = Element.ALIGN_CENTER)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 8,
                BackgroundColor = backgroundColor,
                BorderColor = backgroundColor
            };
        }

        private PdfPCell CreateBodyCell(string text, Font font, int horizontalAlignment, BaseColor backgroundColor)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 8,
                Border = Rectangle.NO_BORDER,
                BackgroundColor = backgroundColor,
                NoWrap = false
            };
        }

        private PdfPCell CreateBodyCell(string text, Font font, int horizontalAlignment, int border = Rectangle.NO_BORDER)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 8,
                Border = border,
                BorderColor = BaseColor.LIGHT_GRAY,
                NoWrap = false
            };
        }

        private PdfPCell CreateSummaryCell(string title, string value, BaseColor borderColor)
        {
            var cell = new PdfPCell { BorderWidth = 0, BorderWidthBottom = 3, BorderColorBottom = borderColor, Padding = 10 };
            var titleFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 9, Font.NORMAL, BaseColor.GRAY);
            var valueFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 16, Font.NORMAL, BaseColor.BLACK);

            cell.AddElement(new Paragraph(title, titleFont));
            cell.AddElement(new Paragraph(value, valueFont));
            return cell;
        }

        private void RefreshPaymentGrid()
        {
            try
            {
                var paymentsWithInfo = _allPayments.Select(p => new
                {
                    p.Id,
                    LeaseInfo = GetLeaseInfo(p.LeaseId),
                    p.Amount,
                    p.PaymentDate,
                    Method = PaymentMethodHelper.GetMethodName(p.Method),
                    p.ReferenceNumber,
                    Status = PaymentStatusHelper.GetStatusName(p.Status),
                    p.Notes,
                    OriginalStatus = p.Status
                })
                .OrderBy(p => p.Id)
                .ToList();
                DataGridPayments.ItemsSource = paymentsWithInfo;
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
                return $"{property?.Name ?? "Unknown"}, {unit?.UnitNumber ?? "N/A"}, {tenant?.FullName ?? "N/A"}";
            }
            return $"Lease {leaseId}";
        }

        private void NotifyParentOfChanges()
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.ManualRefresh();
            }
            MainWindow.NotifyDataChanged();
        }

        private void BtnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            AddEditPaymentWindow addPaymentWindow;
            var selectedTab = this.TabControl.SelectedItem as TabItem;

            // Check if we are on the "By Tenant" tab AND a specific tenant is selected
            if (selectedTab?.Header.ToString() == "By Tenant" &&
                CmbTenantFilter.SelectedValue != null &&
                (int)CmbTenantFilter.SelectedValue != 0)
            {
                // If yes, get the tenant ID and use the new constructor
                int selectedTenantId = (int)CmbTenantFilter.SelectedValue;
                addPaymentWindow = new AddEditPaymentWindow(_allLeases, _allUnits, _allTenants, _allProperties, selectedTenantId);
            }
            else
            {
                // Otherwise, use the old constructor (no pre-selection)
                addPaymentWindow = new AddEditPaymentWindow(_allLeases, _allUnits, _allTenants, _allProperties);
            }

            // The rest of the logic remains the same
            if (addPaymentWindow.ShowDialog() == true)
            {
                LoadAllData();
                NotifyParentOfChanges();
            }
        }

        private void BtnEditPayment_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = this.TabControl.SelectedItem as TabItem;

            // FIX: Use .Contains() to robustly check the header text
            object selectedItem = (selectedTab?.Header.ToString().Contains("By Tenant") == true)
                ? DataGridTenantPayments.SelectedItem
                : DataGridPayments.SelectedItem;

            if (selectedItem != null)
            {
                EditPaymentFromGrid(selectedItem);
            }
            else
            {
                MessageBox.Show("Please select a payment to edit.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void BtnDeletePayment_Click(object sender, RoutedEventArgs e)
        {
            var selectedTab = this.TabControl.SelectedItem as TabItem;

            // FIX: Use .Contains() to robustly check the header text
            object selectedItem = (selectedTab?.Header.ToString().Contains("By Tenant") == true)
                ? DataGridTenantPayments.SelectedItem
                : DataGridPayments.SelectedItem;

            if (selectedItem != null)
            {
                DeletePaymentFromGrid(selectedItem);
            }
            else
            {
                MessageBox.Show("Please select a payment to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void EditPaymentFromGrid(object selectedItem)
        {
            try
            {
                int selectedPaymentId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem, null);
                var paymentToEdit = _allPayments.FirstOrDefault(p => p.Id == selectedPaymentId);
                if (paymentToEdit != null)
                {
                    var editPaymentWindow = new AddEditPaymentWindow(paymentToEdit, _allLeases, _allUnits, _allTenants, _allProperties);
                    if (editPaymentWindow.ShowDialog() == true)
                    {
                        LoadAllData();
                        NotifyParentOfChanges();
                        MessageBox.Show("Payment updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateInvoiceFromGrid(object selectedItem)
        {
            try
            {
                if (selectedItem != null)
                {
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "PDF Files (*.pdf)|*.pdf",
                        FileName = $"Payment_Receipt_{selectedItem.GetType().GetProperty("Id").GetValue(selectedItem)}_{DateTime.Now:yyyyMMdd}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        var itemsList = new List<object> { selectedItem };
                        GenerateBulkPdfInvoice(itemsList, saveDialog.FileName, "Payment Receipt");
                        MessageBox.Show("Receipt generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating receipt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void GenerateBulkInvoiceFromGrid(object itemsSource)
        {
            try
            {
                var itemsList = ((IEnumerable)itemsSource).Cast<object>().ToList();
                if (itemsList.Any())
                {
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "PDF Files (*.pdf)|*.pdf|CSV Files (*.csv)|*.csv",
                        FileName = $"Bulk_Payments_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        if (saveDialog.FileName.EndsWith(".pdf"))
                        {
                            GenerateBulkPdfInvoice(itemsList, saveDialog.FileName);
                            MessageBox.Show($"Bulk PDF invoice generated successfully for {itemsList.Count} payments!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating bulk invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeletePaymentFromGrid(object selectedItem)
        {
            try
            {
                int selectedPaymentId = (int)selectedItem.GetType().GetProperty("Id").GetValue(selectedItem, null);
                var paymentToDelete = _allPayments.FirstOrDefault(p => p.Id == selectedPaymentId);
                if (paymentToDelete != null)
                {
                    var leaseInfo = GetLeaseInfo(paymentToDelete.LeaseId);
                    var result = MessageBox.Show($"Are you sure you want to delete the payment for {leaseInfo}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _paymentRepository.Delete(paymentToDelete.Id);
                        LoadAllData();
                        NotifyParentOfChanges();
                        MessageBox.Show("Payment deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbStatusFilter.SelectedValue != null)
            {
                var selectedStatus = (int)CmbStatusFilter.SelectedValue;
                var filteredPayments = _allPayments.Where(p => (int)p.Status == selectedStatus).ToList();
                RefreshFilteredPaymentGrid(filteredPayments);
            }
        }

        private void CmbMethodFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbMethodFilter.SelectedValue != null)
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
            })
                .OrderBy(p => p.Id)
                .ToList();
            DataGridPayments.ItemsSource = paymentsWithInfo;
        }
    }

    public class PdfPageEvents : PdfPageEventHelper
    {
        private readonly Font _footerFont = new Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 8, Font.ITALIC);
        private readonly string _reportTitle;

        public PdfPageEvents(string reportTitle)
        {
            _reportTitle = reportTitle;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            var footerTable = new PdfPTable(2) { TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin };
            footerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var leftCell = new PdfPCell(new Phrase($"{_reportTitle} - Generated on {DateTime.Now:yyyy-MM-dd HH:mm}", _footerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            var rightCell = new PdfPCell(new Phrase($"Page {writer.PageNumber}", _footerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            footerTable.AddCell(leftCell);
            footerTable.AddCell(rightCell);

            footerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 10, writer.DirectContent);
        }
    }
}