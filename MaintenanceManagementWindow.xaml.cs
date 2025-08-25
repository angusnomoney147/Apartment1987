using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ApartmentManagementSystem
{
    public partial class MaintenanceManagementWindow : Window
    {
        private readonly MaintenanceRequestRepository _maintenanceRepository;
        private List<MaintenanceRequest> _allRequests = new();
        private List<MaintenanceRequest> _filteredRequests = new();

        public MaintenanceManagementWindow()
        {
            InitializeComponent();
            _maintenanceRepository = new MaintenanceRequestRepository();
            InitializeFilters();
            LoadMaintenanceRequests();
        }

        private void InitializeFilters()
        {
            // Initialize status filter
            var statuses = Enum.GetValues(typeof(MaintenanceStatus)).Cast<MaintenanceStatus>()
                .Select(s => new { Value = (int)s, Name = s.ToString() })
                .ToList();
            statuses.Insert(0, new { Value = -1, Name = "All Statuses" });
            CmbStatusFilter.ItemsSource = statuses;
            CmbStatusFilter.DisplayMemberPath = "Name";
            CmbStatusFilter.SelectedValuePath = "Value";
            CmbStatusFilter.SelectedIndex = 0;

            // Initialize priority filter
            var priorities = Enum.GetValues(typeof(MaintenancePriority)).Cast<MaintenancePriority>()
                .Select(p => new { Value = (int)p, Name = p.ToString() })
                .ToList();
            priorities.Insert(0, new { Value = -1, Name = "All Priorities" });
            CmbPriorityFilter.ItemsSource = priorities;
            CmbPriorityFilter.DisplayMemberPath = "Name";
            CmbPriorityFilter.SelectedValuePath = "Value";
            CmbPriorityFilter.SelectedIndex = 0;
        }

        private void LoadMaintenanceRequests()
        {
            try
            {
                _allRequests = _maintenanceRepository.GetAll();
                _filteredRequests = new List<MaintenanceRequest>(_allRequests);
                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance requests: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDataGrid()
        {
            var requestsWithInfo = _filteredRequests.Select(r => new
            {
                r.Id,
                UnitInfo = $"{r.UnitNumber} ({r.PropertyName})",
                TenantInfo = r.TenantName ?? "Unoccupied",
                r.Description,
                Priority = r.Priority.ToString(),
                Status = r.Status.ToString(),
                RequestDate = r.RequestDate,
                CompletedDate = r.CompletedDate,
                r.Cost,
                r.AssignedTo
            }).ToList();

            DataGridRequests.ItemsSource = requestsWithInfo;
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbPriorityFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _filteredRequests = new List<MaintenanceRequest>(_allRequests);

            // Apply status filter
            if (CmbStatusFilter.SelectedValue != null && (int)CmbStatusFilter.SelectedValue != -1)
            {
                var statusValue = (int)CmbStatusFilter.SelectedValue;
                _filteredRequests = _filteredRequests.Where(r => (int)r.Status == statusValue).ToList();
            }

            // Apply priority filter
            if (CmbPriorityFilter.SelectedValue != null && (int)CmbPriorityFilter.SelectedValue != -1)
            {
                var priorityValue = (int)CmbPriorityFilter.SelectedValue;
                _filteredRequests = _filteredRequests.Where(r => (int)r.Priority == priorityValue).ToList();
            }

            RefreshDataGrid();
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            CmbStatusFilter.SelectedIndex = 0;
            CmbPriorityFilter.SelectedIndex = 0;
            _filteredRequests = new List<MaintenanceRequest>(_allRequests);
            RefreshDataGrid();
        }

        private void BtnPendingOnly_Click(object sender, RoutedEventArgs e)
        {
            CmbStatusFilter.SelectedValue = (int)MaintenanceStatus.Pending;
            CmbPriorityFilter.SelectedIndex = 0;
            _filteredRequests = _allRequests.Where(r => r.Status == MaintenanceStatus.Pending).ToList();
            RefreshDataGrid();
        }

        private void BtnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            var addRequestWindow = new AddEditMaintenanceWindow();
            if (addRequestWindow.ShowDialog() == true)
            {
                LoadMaintenanceRequests();
                MainWindow.NotifyDataChanged(); // Add this line
            }
        }

        private void BtnMarkAsComplete_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridRequests.SelectedItem != null)
            {
                var selectedItem = DataGridRequests.SelectedItem;
                var requestIdProperty = selectedItem.GetType().GetProperty("Id");
                int requestId = requestIdProperty != null ? (int)requestIdProperty.GetValue(selectedItem) : 0;

                if (requestId > 0)
                {
                    var request = _allRequests.FirstOrDefault(r => r.Id == requestId);
                    if (request != null && request.Status != MaintenanceStatus.Completed)
                    {
                        var result = MessageBox.Show("Mark this request as completed?", "Confirm",
                                                   MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                request.Status = MaintenanceStatus.Completed;
                                request.CompletedDate = DateTime.Now;
                                _maintenanceRepository.Update(request);
                                LoadMaintenanceRequests();
                                MainWindow.NotifyDataChanged(); // Add this line
                                MessageBox.Show("Request marked as completed!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error updating request: {ex.Message}", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Request is already completed.", "Information",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a request to mark as complete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnEditRequest_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridRequests.SelectedItem != null)
            {
                var selectedItem = DataGridRequests.SelectedItem;
                var requestIdProperty = selectedItem.GetType().GetProperty("Id");
                int requestId = requestIdProperty != null ? (int)requestIdProperty.GetValue(selectedItem) : 0;

                if (requestId > 0)
                {
                    var requestToEdit = _allRequests.FirstOrDefault(r => r.Id == requestId);
                    if (requestToEdit != null)
                    {
                        var editWindow = new AddEditMaintenanceWindow(requestToEdit);
                        if (editWindow.ShowDialog() == true)
                        {
                            LoadMaintenanceRequests();
                            MainWindow.NotifyDataChanged(); // Add this line
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a request to edit.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridRequests.SelectedItem != null)
            {
                var selectedItem = DataGridRequests.SelectedItem;
                var requestIdProperty = selectedItem.GetType().GetProperty("Id");
                int requestId = requestIdProperty != null ? (int)requestIdProperty.GetValue(selectedItem) : 0;

                if (requestId > 0)
                {
                    var requestToDelete = _allRequests.FirstOrDefault(r => r.Id == requestId);
                    if (requestToDelete != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete this maintenance request?\n\n{requestToDelete.Description}",
                                                   "Confirm Delete",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                _maintenanceRepository.Delete(requestId);
                                LoadMaintenanceRequests();
                                MainWindow.NotifyDataChanged(); // Add this line
                                MessageBox.Show("Request deleted successfully!", "Success",
                                              MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting request: {ex.Message}", "Error",
                                              MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a request to delete.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = TxtSearch.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredRequests = new List<MaintenanceRequest>(_allRequests);
            }
            else
            {
                _filteredRequests = _allRequests.Where(r =>
                    (r.UnitNumber?.ToLower().Contains(searchText) ?? false) ||
                    (r.PropertyName?.ToLower().Contains(searchText) ?? false) ||
                    (r.TenantName?.ToLower().Contains(searchText) ?? false) ||
                    (r.Description?.ToLower().Contains(searchText) ?? false) ||
                    (r.AssignedTo?.ToLower().Contains(searchText) ?? false)).ToList();
            }
            ApplyFilters();
        }
    }
}