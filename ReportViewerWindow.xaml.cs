using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace ApartmentManagementSystem
{
    public partial class ReportViewerWindow : Window
    {
        private string _reportContent;
        private string _reportTitle;

        public ReportViewerWindow(string title, string content)
        {
            InitializeComponent();
            _reportTitle = title;
            _reportContent = content;
            TxtTitle.Text = title;
            TxtReportContent.Text = content;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality would be implemented here.", "Print",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportTxt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"{_reportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, _reportContent);
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}",
                                  "Export Successful",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                    FileName = $"{_reportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // This would need to be implemented based on the specific report type
                    MessageBox.Show("PDF export functionality would be implemented here.",
                                  "Export Info",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}", "Export Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                    FileName = $"{_reportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // This would need to be implemented based on the specific report type
                    MessageBox.Show("Excel export functionality would be implemented here.",
                                  "Export Info",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting Excel: {ex.Message}", "Export Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}