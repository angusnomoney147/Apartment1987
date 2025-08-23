using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;

namespace ApartmentManagementSystem
{
    public class MaintenanceRepository
    {
        private readonly string _connectionString;

        public MaintenanceRepository()
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
                            var request = new MaintenanceRequest
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UnitId = Convert.ToInt32(reader["UnitId"]),
                                TenantId = Convert.ToInt32(reader["TenantId"]),
                                UnitNumber = reader["UnitNumber"].ToString(),
                                PropertyName = reader["PropertyName"].ToString(),
                                TenantName = reader["TenantName"].ToString(),
                                Description = reader["Description"].ToString(),
                                Priority = (MaintenancePriority)Enum.Parse(typeof(MaintenancePriority), reader["Priority"].ToString()),
                                Status = (MaintenanceStatus)Enum.Parse(typeof(MaintenanceStatus), reader["Status"].ToString()),
                                RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                                CompletedDate = reader["CompletedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CompletedDate"]),
                                Cost = reader["Cost"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Cost"]),
                                AssignedTo = reader["AssignedTo"].ToString()
                            };
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
                                return new MaintenanceRequest
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UnitId = Convert.ToInt32(reader["UnitId"]),
                                    TenantId = Convert.ToInt32(reader["TenantId"]),
                                    UnitNumber = reader["UnitNumber"].ToString(),
                                    PropertyName = reader["PropertyName"].ToString(),
                                    TenantName = reader["TenantName"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    Priority = (MaintenancePriority)Enum.Parse(typeof(MaintenancePriority), reader["Priority"].ToString()),
                                    Status = (MaintenanceStatus)Enum.Parse(typeof(MaintenanceStatus), reader["Status"].ToString()),
                                    RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CompletedDate"]),
                                    Cost = reader["Cost"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Cost"]),
                                    AssignedTo = reader["AssignedTo"].ToString()
                                };
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
                        command.Parameters.AddWithValue("@UnitNumber", request.UnitNumber);
                        command.Parameters.AddWithValue("@PropertyName", request.PropertyName);
                        command.Parameters.AddWithValue("@TenantName", request.TenantName);
                        command.Parameters.AddWithValue("@Description", request.Description);
                        command.Parameters.AddWithValue("@Priority", request.Priority.ToString());
                        command.Parameters.AddWithValue("@Status", request.Status.ToString());
                        command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
                        command.Parameters.AddWithValue("@CompletedDate", (object)request.CompletedDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Cost", (object)request.Cost ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AssignedTo", request.AssignedTo);

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
                        command.Parameters.AddWithValue("@UnitNumber", request.UnitNumber);
                        command.Parameters.AddWithValue("@PropertyName", request.PropertyName);
                        command.Parameters.AddWithValue("@TenantName", request.TenantName);
                        command.Parameters.AddWithValue("@Description", request.Description);
                        command.Parameters.AddWithValue("@Priority", request.Priority.ToString());
                        command.Parameters.AddWithValue("@Status", request.Status.ToString());
                        command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
                        command.Parameters.AddWithValue("@CompletedDate", (object)request.CompletedDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Cost", (object)request.Cost ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AssignedTo", request.AssignedTo);

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
                                var request = new MaintenanceRequest
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UnitId = Convert.ToInt32(reader["UnitId"]),
                                    TenantId = Convert.ToInt32(reader["TenantId"]),
                                    UnitNumber = reader["UnitNumber"].ToString(),
                                    PropertyName = reader["PropertyName"].ToString(),
                                    TenantName = reader["TenantName"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    Priority = (MaintenancePriority)Enum.Parse(typeof(MaintenancePriority), reader["Priority"].ToString()),
                                    Status = (MaintenanceStatus)Enum.Parse(typeof(MaintenanceStatus), reader["Status"].ToString()),
                                    RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                                    CompletedDate = reader["CompletedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CompletedDate"]),
                                    Cost = reader["Cost"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Cost"]),
                                    AssignedTo = reader["AssignedTo"].ToString()
                                };
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
    }
}