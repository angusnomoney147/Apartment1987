using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;

namespace ApartmentManagementSystem
{
    public class MaintenanceRequestRepository
    {
        private readonly string _connectionString;

        public MaintenanceRequestRepository()
        {
            _connectionString = "Data Source=apartment.db;Version=3;";
        }

        public List<MaintenanceRequest> GetAll()
        {
            var requests = new List<MaintenanceRequest>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM MaintenanceRequests ORDER BY RequestDate DESC", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var request = new MaintenanceRequest();

                            // Safely read each column
                            request.Id = GetInt32(reader, "Id", 0);
                            request.UnitId = GetInt32(reader, "UnitId", 0);
                            request.TenantId = GetInt32(reader, "TenantId", 0);
                            request.UnitNumber = GetString(reader, "UnitNumber", "");
                            request.PropertyName = GetString(reader, "PropertyName", "");
                            request.TenantName = GetString(reader, "TenantName", "");
                            request.Description = GetString(reader, "Description", "");

                            // Handle enums safely
                            request.Priority = GetEnum<MaintenancePriority>(reader, "Priority", MaintenancePriority.Medium);
                            request.Status = GetEnum<MaintenanceStatus>(reader, "Status", MaintenanceStatus.Pending);

                            request.RequestDate = GetDateTime(reader, "RequestDate", DateTime.Now);
                            request.CompletedDate = GetNullableDateTime(reader, "CompletedDate");
                            request.Cost = GetNullableDecimal(reader, "Cost");
                            request.AssignedTo = GetString(reader, "AssignedTo", "");

                            requests.Add(request);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance requests: {ex.Message}");
            }
            return requests;
        }

        public MaintenanceRequest GetById(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM MaintenanceRequests WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var request = new MaintenanceRequest();

                                request.Id = GetInt32(reader, "Id", 0);
                                request.UnitId = GetInt32(reader, "UnitId", 0);
                                request.TenantId = GetInt32(reader, "TenantId", 0);
                                request.UnitNumber = GetString(reader, "UnitNumber", "");
                                request.PropertyName = GetString(reader, "PropertyName", "");
                                request.TenantName = GetString(reader, "TenantName", "");
                                request.Description = GetString(reader, "Description", "");

                                request.Priority = GetEnum<MaintenancePriority>(reader, "Priority", MaintenancePriority.Medium);
                                request.Status = GetEnum<MaintenanceStatus>(reader, "Status", MaintenanceStatus.Pending);

                                request.RequestDate = GetDateTime(reader, "RequestDate", DateTime.Now);
                                request.CompletedDate = GetNullableDateTime(reader, "CompletedDate");
                                request.Cost = GetNullableDecimal(reader, "Cost");
                                request.AssignedTo = GetString(reader, "AssignedTo", "");

                                return request;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance request: {ex.Message}");
            }
            return null;
        }

        public void Add(MaintenanceRequest request)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(@"
                        INSERT INTO MaintenanceRequests 
                        (UnitId, TenantId, UnitNumber, PropertyName, TenantName, Description, Priority, Status, RequestDate, CompletedDate, Cost, AssignedTo)
                        VALUES 
                        (@UnitId, @TenantId, @UnitNumber, @PropertyName, @TenantName, @Description, @Priority, @Status, @RequestDate, @CompletedDate, @Cost, @AssignedTo)", connection))
                    {
                        command.Parameters.AddWithValue("@UnitId", request.UnitId);
                        command.Parameters.AddWithValue("@TenantId", request.TenantId);
                        command.Parameters.AddWithValue("@UnitNumber", request.UnitNumber ?? "");
                        command.Parameters.AddWithValue("@PropertyName", request.PropertyName ?? "");
                        command.Parameters.AddWithValue("@TenantName", request.TenantName ?? "");
                        command.Parameters.AddWithValue("@Description", request.Description ?? "");
                        command.Parameters.AddWithValue("@Priority", request.Priority.ToString());
                        command.Parameters.AddWithValue("@Status", request.Status.ToString());
                        command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
                        command.Parameters.AddWithValue("@CompletedDate", (object)request.CompletedDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Cost", (object)request.Cost ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AssignedTo", request.AssignedTo ?? "");

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding maintenance request: {ex.Message}");
            }
        }

        public void Update(MaintenanceRequest request)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(@"
                        UPDATE MaintenanceRequests SET
                        UnitId = @UnitId,
                        TenantId = @TenantId,
                        UnitNumber = @UnitNumber,
                        PropertyName = @PropertyName,
                        TenantName = @TenantName,
                        Description = @Description,
                        Priority = @Priority,
                        Status = @Status,
                        RequestDate = @RequestDate,
                        CompletedDate = @CompletedDate,
                        Cost = @Cost,
                        AssignedTo = @AssignedTo
                        WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", request.Id);
                        command.Parameters.AddWithValue("@UnitId", request.UnitId);
                        command.Parameters.AddWithValue("@TenantId", request.TenantId);
                        command.Parameters.AddWithValue("@UnitNumber", request.UnitNumber ?? "");
                        command.Parameters.AddWithValue("@PropertyName", request.PropertyName ?? "");
                        command.Parameters.AddWithValue("@TenantName", request.TenantName ?? "");
                        command.Parameters.AddWithValue("@Description", request.Description ?? "");
                        command.Parameters.AddWithValue("@Priority", request.Priority.ToString());
                        command.Parameters.AddWithValue("@Status", request.Status.ToString());
                        command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
                        command.Parameters.AddWithValue("@CompletedDate", (object)request.CompletedDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Cost", (object)request.Cost ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AssignedTo", request.AssignedTo ?? "");

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating maintenance request: {ex.Message}");
            }
        }

        public void Delete(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("DELETE FROM MaintenanceRequests WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting maintenance request: {ex.Message}");
            }
        }

        public List<MaintenanceRequest> GetByStatus(MaintenanceStatus status)
        {
            var requests = new List<MaintenanceRequest>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM MaintenanceRequests WHERE Status = @Status ORDER BY RequestDate DESC", connection))
                    {
                        command.Parameters.AddWithValue("@Status", status.ToString());
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var request = new MaintenanceRequest();

                                request.Id = GetInt32(reader, "Id", 0);
                                request.UnitId = GetInt32(reader, "UnitId", 0);
                                request.TenantId = GetInt32(reader, "TenantId", 0);
                                request.UnitNumber = GetString(reader, "UnitNumber", "");
                                request.PropertyName = GetString(reader, "PropertyName", "");
                                request.TenantName = GetString(reader, "TenantName", "");
                                request.Description = GetString(reader, "Description", "");

                                request.Priority = GetEnum<MaintenancePriority>(reader, "Priority", MaintenancePriority.Medium);
                                request.Status = GetEnum<MaintenanceStatus>(reader, "Status", MaintenanceStatus.Pending);

                                request.RequestDate = GetDateTime(reader, "RequestDate", DateTime.Now);
                                request.CompletedDate = GetNullableDateTime(reader, "CompletedDate");
                                request.Cost = GetNullableDecimal(reader, "Cost");
                                request.AssignedTo = GetString(reader, "AssignedTo", "");

                                requests.Add(request);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance requests: {ex.Message}");
            }
            return requests;
        }

        // Helper methods to safely read data
        private int GetInt32(SQLiteDataReader reader, string columnName, int defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) ? reader.GetInt32(ordinal) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private string GetString(SQLiteDataReader reader, string columnName, string defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) ? reader.GetString(ordinal) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private DateTime GetDateTime(SQLiteDataReader reader, string columnName, DateTime defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) ? reader.GetDateTime(ordinal) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private DateTime? GetNullableDateTime(SQLiteDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) ? reader.GetDateTime(ordinal) : (DateTime?)null;
            }
            catch
            {
                return null;
            }
        }

        private decimal? GetNullableDecimal(SQLiteDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) ? (decimal?)reader.GetDecimal(ordinal) : null;
            }
            catch
            {
                return null;
            }
        }

        private T GetEnum<T>(SQLiteDataReader reader, string columnName, T defaultValue) where T : struct
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (!reader.IsDBNull(ordinal))
                {
                    var value = reader.GetString(ordinal);
                    if (Enum.TryParse<T>(value, out T result))
                    {
                        return result;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}