using System;
using System.Linq;
using System.Windows;

namespace ApartmentManagementSystem
{
    public static class UnitStatusManager
    {
        public static void UpdateUnitStatusAfterMaintenance(int unitId, UnitRepository unitRepository)
        {
            try
            {
                var unit = unitRepository.GetAll().FirstOrDefault(u => u.Id == unitId);
                if (unit != null && unit.Status == UnitStatus.Maintenance)
                {
                    // Check if there are any other pending maintenance requests for this unit
                    var maintenanceRepository = new MaintenanceRepository();
                    var pendingRequests = maintenanceRepository.GetAll()
                        .Where(r => r.UnitId == unitId &&
                                   (r.Status == MaintenanceStatus.Pending || r.Status == MaintenanceStatus.InProgress))
                        .ToList();

                    if (!pendingRequests.Any())
                    {
                        // No more pending maintenance, update unit to Vacant
                        unit.Status = UnitStatus.Vacant;
                        unitRepository.Update(unit);

                        MessageBox.Show($"Unit {unit.UnitNumber} has been automatically updated to Vacant status as all maintenance is completed.",
                                      "Unit Status Updated",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Unit {unit.UnitNumber} still has {pendingRequests.Count} pending maintenance requests.",
                                      "Maintenance Pending",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating unit status: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}