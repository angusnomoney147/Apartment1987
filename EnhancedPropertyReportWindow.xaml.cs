using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;

// iTextSharp references
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ApartmentManagementSystem
{
    public partial class EnhancedPropertyReportWindow : Window
    {
        private readonly PropertyRepository _propertyRepository;
        private readonly UnitRepository _unitRepository;
        private List<Property> _allProperties;
        private List<Unit> _allUnits;

        public EnhancedPropertyReportWindow()
        {
            InitializeComponent();
            _propertyRepository = new PropertyRepository();
            _unitRepository = new UnitRepository();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            try
            {
                _allProperties = _propertyRepository.GetAll();
                _allUnits = _unitRepository.GetAll();

                // Populate property filter
                PopulatePropertyFilter();

                // Generate initial report
                GenerateReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading  {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulatePropertyFilter()
        {
            var properties = new List<object> { new { Id = 0, Name = "All Properties" } };
            properties.AddRange(_allProperties.OrderBy(p => p.Name).Select(p => new { p.Id, p.Name }));

            CmbProperties.ItemsSource = properties;
            CmbProperties.DisplayMemberPath = "Name";
            CmbProperties.SelectedValuePath = "Id";
            CmbProperties.SelectedIndex = 0;
        }

        private void GenerateReport()
        {
            try
            {
                var propertyId = CmbProperties.SelectedValue != null ? (int)CmbProperties.SelectedValue : 0;

                // Filter properties
                var filteredProperties = propertyId > 0
                    ? _allProperties.Where(p => p.Id == propertyId).ToList()
                    : _allProperties;

                // Generate report data
                var reportData = filteredProperties.Select(p =>
                {
                    var propertyUnits = _allUnits.Where(u => u.PropertyId == p.Id).ToList();
                    var vacantUnits = propertyUnits.Count(u => u.Status == UnitStatus.Vacant);
                    var occupiedUnits = propertyUnits.Count(u => u.Status == UnitStatus.Occupied);

                    return new
                    {
                        PropertyName = p.Name,
                        TotalUnits = propertyUnits.Count,
                        VacantUnits = vacantUnits,
                        OccupiedUnits = occupiedUnits,
                        Address = p.Address ?? "N/A",
                        City = p.City ?? "N/A",
                        ManagerName = p.ManagerName ?? "N/A",
                        ContactInfo = p.ContactInfo ?? "N/A"
                    };
                })
                .OrderBy(p => p.PropertyName) // Order by property name
                .ToList();

                DataGridPropertyReport.ItemsSource = reportData;

                // Update summary
                TxtSummary.Text = $"Total Properties: {reportData.Count}";
                TxtTotalUnits.Text = $"Total Units: {reportData.Sum(p => p.TotalUnits)}";
                TxtVacantUnits.Text = $"Vacant Units: {reportData.Sum(p => p.VacantUnits)}";
                TxtOccupiedUnits.Text = $"Occupied Units: {reportData.Sum(p => p.OccupiedUnits)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateReport();
        }

        private void CmbProperties_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateReport();
        }

        // New PDF Export Button Handler
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"PropertyParams_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var reportData = DataGridPropertyReport.ItemsSource.Cast<object>().ToList();
                    ExportToProfessionalPdf(reportData, saveDialog.FileName);
                    MessageBox.Show("PDF report generated successfully!", "Success",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // New Excel Export Button Handler
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"PropertyParams_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var reportData = DataGridPropertyReport.ItemsSource.Cast<object>().ToList();

                    if (saveDialog.FileName.EndsWith(".csv"))
                    {
                        ExportToExcel(reportData, saveDialog.FileName);
                        MessageBox.Show("Excel-compatible report generated successfully!", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting Excel: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Original Multi-Format Export Button Handler
        private void ExportToExcel(List<object> data, string fileName)
        {
            try
            {
                var csv = new System.Text.StringBuilder();

                // Add Excel-compatible CSV header
                csv.AppendLine("PropertyParams Report");
                csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                csv.AppendLine();

                // Add column headers
                csv.AppendLine("PropertyParams,Total Units,Vacant Units,Occupied Units,Address,City,Manager,Phone");

                // Add data rows
                foreach (var item in data)
                {
                    var itemType = item.GetType();
                    var propertyName = itemType.GetProperty("PropertyName")?.GetValue(item)?.ToString() ?? "";
                    var totalUnits = itemType.GetProperty("TotalUnits")?.GetValue(item)?.ToString() ?? "";
                    var vacantUnits = itemType.GetProperty("VacantUnits")?.GetValue(item)?.ToString() ?? "";
                    var occupiedUnits = itemType.GetProperty("OccupiedUnits")?.GetValue(item)?.ToString() ?? "";
                    var address = itemType.GetProperty("Address")?.GetValue(item)?.ToString() ?? "";
                    var city = itemType.GetProperty("City")?.GetValue(item)?.ToString() ?? "";
                    var managerName = itemType.GetProperty("ManagerName")?.GetValue(item)?.ToString() ?? "";
                    var contactInfo = itemType.GetProperty("ContactInfo")?.GetValue(item)?.ToString() ?? "";

                    csv.AppendLine($"\"{propertyName}\",\"{totalUnits}\",\"{vacantUnits}\",\"{occupiedUnits}\",\"{address}\",\"{city}\",\"{managerName}\",\"{contactInfo}\"");
                }

                System.IO.File.WriteAllText(fileName, csv.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating Excel report: {ex.Message}", ex);
            }
        }

        private void ExportToProfessionalPdf(List<object> data, string fileName)
        {
            try
            {
                var document = new Document(iTextSharp.text.PageSize.A4.Rotate(), 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                document.Open();

                // Company Header
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                // Company Logo/Header
                var companyHeader = new Paragraph("PropertyParams MANAGEMENT REPORT", headerFont)
                {
                    Alignment = iTextSharp.text.Element.ALIGN_CENTER,
                    SpacingAfter = 5
                };
                document.Add(companyHeader);

                var companySubHeader = new Paragraph("Property Portfolio Analysis", subHeaderFont)
                {
                    Alignment = iTextSharp.text.Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(companySubHeader);

                // Report Info
                var reportInfo = new Paragraph($"Generated on: {DateTime.Now:MMMM dd, yyyy HH:mm}", normalFont)
                {
                    Alignment = iTextSharp.text.Element.ALIGN_RIGHT,
                    SpacingAfter = 20
                };
                document.Add(reportInfo);

                // Summary Section
                var summaryTitle = new Paragraph("PORTFOLIO SUMMARY", boldFont)
                {
                    SpacingAfter = 10
                };
                document.Add(summaryTitle);

                var summaryTable = new PdfPTable(4) { WidthPercentage = 100 };
                summaryTable.SetWidths(new float[] { 1f, 1f, 1f, 1f });

                // Summary Headers
                summaryTable.AddCell(CreatePdfCell("Total Properties", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                summaryTable.AddCell(CreatePdfCell("Total Units", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                summaryTable.AddCell(CreatePdfCell("Vacant Units", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                summaryTable.AddCell(CreatePdfCell("Occupied Units", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));

                // Summary Data
                summaryTable.AddCell(CreatePdfCell(TxtSummary.Text.Replace("Total Properties: ", ""), normalFont));
                summaryTable.AddCell(CreatePdfCell(TxtTotalUnits.Text.Replace("Total Units: ", ""), normalFont));
                summaryTable.AddCell(CreatePdfCell(TxtVacantUnits.Text.Replace("Vacant Units: ", ""), normalFont));
                summaryTable.AddCell(CreatePdfCell(TxtOccupiedUnits.Text.Replace("Occupied Units: ", ""), normalFont));

                document.Add(summaryTable);
                document.Add(new Paragraph(" ") { SpacingAfter = 15 });

                // Detailed Property Report
                var detailTitle = new Paragraph("DETAILED PROPERTY ANALYSIS", boldFont)
                {
                    SpacingAfter = 10
                };
                document.Add(detailTitle);

                // Detailed Table
                var detailTable = new PdfPTable(8) { WidthPercentage = 100 };
                detailTable.SetWidths(new float[] { 2f, 1f, 1f, 1f, 2f, 1f, 1.5f, 1.5f });

                // Table Headers
                detailTable.AddCell(CreatePdfCell("PropertyParams", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                detailTable.AddCell(CreatePdfCell("Total Units", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                detailTable.AddCell(CreatePdfCell("Vacant", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                detailTable.AddCell(CreatePdfCell("Occupied", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                detailTable.AddCell(CreatePdfCell("Address", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                detailTable.AddCell(CreatePdfCell("City", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                detailTable.AddCell(CreatePdfCell("Manager", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));
                detailTable.AddCell(CreatePdfCell("Phone", boldFont, iTextSharp.text.BaseColor.LIGHT_GRAY));

                // Table Data
                foreach (var item in data)
                {
                    var itemType = item.GetType();
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("PropertyName")?.GetValue(item)?.ToString() ?? "", normalFont));
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("TotalUnits")?.GetValue(item)?.ToString() ?? "", normalFont));
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("VacantUnits")?.GetValue(item)?.ToString() ?? "", normalFont));
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("OccupiedUnits")?.GetValue(item)?.ToString() ?? "", normalFont));
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("Address")?.GetValue(item)?.ToString() ?? "", normalFont));
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("City")?.GetValue(item)?.ToString() ?? "", normalFont));
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("ManagerName")?.GetValue(item)?.ToString() ?? "", normalFont));
                    detailTable.AddCell(CreatePdfCell(itemType.GetProperty("ContactInfo")?.GetValue(item)?.ToString() ?? "", normalFont));
                }

                document.Add(detailTable);

                // Footer
                document.Add(new Paragraph(" ") { SpacingBefore = 20 });
                var footer = new Paragraph("CONFIDENTIAL - Property Management Report", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8))
                {
                    Alignment = iTextSharp.text.Element.ALIGN_CENTER
                };
                document.Add(footer);

                document.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating PDF: {ex.Message}", ex);
            }
        }

        private PdfPCell CreatePdfCell(string text, Font font, iTextSharp.text.BaseColor backgroundColor = null)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                Padding = 5,
                HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT
            };

            if (backgroundColor != null)
            {
                cell.BackgroundColor = backgroundColor;
            }

            return cell;
        }

        private void ExportToCsv(List<object> data, string fileName)
        {
            var csv = new System.Text.StringBuilder();

            // Add header
            csv.AppendLine("PropertyParams,Total Units,Vacant Units,Occupied Units,Address,City,Manager,Phone");

            // Add data rows
            foreach (var item in data)
            {
                var itemType = item.GetType();
                var propertyName = itemType.GetProperty("PropertyName")?.GetValue(item)?.ToString() ?? "";
                var totalUnits = itemType.GetProperty("TotalUnits")?.GetValue(item)?.ToString() ?? "";
                var vacantUnits = itemType.GetProperty("VacantUnits")?.GetValue(item)?.ToString() ?? "";
                var occupiedUnits = itemType.GetProperty("OccupiedUnits")?.GetValue(item)?.ToString() ?? "";
                var address = itemType.GetProperty("Address")?.GetValue(item)?.ToString() ?? "";
                var city = itemType.GetProperty("City")?.GetValue(item)?.ToString() ?? "";
                var managerName = itemType.GetProperty("ManagerName")?.GetValue(item)?.ToString() ?? "";
                var contactInfo = itemType.GetProperty("ContactInfo")?.GetValue(item)?.ToString() ?? "";

                csv.AppendLine($"\"{propertyName}\",\"{totalUnits}\",\"{vacantUnits}\",\"{occupiedUnits}\",\"{address}\",\"{city}\",\"{managerName}\",\"{contactInfo}\"");
            }

            System.IO.File.WriteAllText(fileName, csv.ToString());
        }

        private void ExportToText(List<object> data, string fileName)
        {
            var text = new System.Text.StringBuilder();
            text.AppendLine("PropertyParams MANAGEMENT REPORT");
            text.AppendLine("===============================");
            text.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            text.AppendLine();

            foreach (var item in data)
            {
                var itemType = item.GetType();
                var propertyName = itemType.GetProperty("PropertyName")?.GetValue(item)?.ToString() ?? "";
                var totalUnits = itemType.GetProperty("TotalUnits")?.GetValue(item)?.ToString() ?? "";
                var vacantUnits = itemType.GetProperty("VacantUnits")?.GetValue(item)?.ToString() ?? "";
                var occupiedUnits = itemType.GetProperty("OccupiedUnits")?.GetValue(item)?.ToString() ?? "";
                var address = itemType.GetProperty("Address")?.GetValue(item)?.ToString() ?? "";
                var city = itemType.GetProperty("City")?.GetValue(item)?.ToString() ?? "";
                var managerName = itemType.GetProperty("ManagerName")?.GetValue(item)?.ToString() ?? "";
                var contactInfo = itemType.GetProperty("ContactInfo")?.GetValue(item)?.ToString() ?? "";

                text.AppendLine($"PropertyParams: {propertyName}");
                text.AppendLine($"  Total Units: {totalUnits}");
                text.AppendLine($"  Vacant Units: {vacantUnits}");
                text.AppendLine($"  Occupied Units: {occupiedUnits}");
                text.AppendLine($"  Address: {address}");
                text.AppendLine($"  City: {city}");
                text.AppendLine($"  Manager: {managerName}");
                text.AppendLine($"  Phone: {contactInfo}");
                text.AppendLine();
            }

            System.IO.File.WriteAllText(fileName, text.ToString());
        }
    }
}