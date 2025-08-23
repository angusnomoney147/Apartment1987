using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ApartmentManagementSystem
{
    public class MaintenanceRepository
    {
        private readonly string _connectionString;

        public MaintenanceRepository()
        {
            _connectionString = DatabaseHelper.GetConnectionString();
        }

        public List<MaintenanceRequest> GetAll()
        {
            var requests = new List<MaintenanceRequest>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM MaintenanceRequests ORDER BY RequestDate DESC", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                requests.Add(new MaintenanceRequest
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    UnitId = Convert.ToInt32(reader["UnitId"]),
                    TenantId = Convert.ToInt32(reader["TenantId"]),
                    Description = reader["Description"]?.ToString() ?? string.Empty,
                    Priority = (MaintenancePriority)Convert.ToInt32(reader["Priority"] ?? 1),
                    Status = (MaintenanceStatus)Convert.ToInt32(reader["Status"] ?? 0),
                    RequestDate = DateTime.Parse(reader["RequestDate"]?.ToString() ?? DateTime.Now.ToString()),
                    CompletedDate = reader["CompletedDate"] != DBNull.Value ?
                        DateTime.Parse(reader["CompletedDate"].ToString()) : (DateTime?)null,
                    Cost = reader["Cost"] != DBNull.Value ?
                        Convert.ToDecimal(reader["Cost"]) : (decimal?)null,
                    AssignedTo = reader["AssignedTo"]?.ToString() ?? string.Empty
                });
            }
            return requests;
        }

        public void Add(MaintenanceRequest request)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO MaintenanceRequests (UnitId, TenantId, Description, 
                                 Priority, Status, RequestDate, CompletedDate, Cost, AssignedTo) 
                                 VALUES (@UnitId, @TenantId, @Description, 
                                 @Priority, @Status, @RequestDate, @CompletedDate, @Cost, @AssignedTo)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UnitId", request.UnitId);
            command.Parameters.AddWithValue("@TenantId", request.TenantId);
            command.Parameters.AddWithValue("@Description", request.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Priority", (int)request.Priority);
            command.Parameters.AddWithValue("@Status", (int)request.Status);
            command.Parameters.AddWithValue("@RequestDate", request.RequestDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@CompletedDate", request.CompletedDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Cost", request.Cost.HasValue ? (double)request.Cost.Value : (object)DBNull.Value);
            command.Parameters.AddWithValue("@AssignedTo", request.AssignedTo ?? (object)DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Update(MaintenanceRequest request)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE MaintenanceRequests SET UnitId = @UnitId, TenantId = @TenantId, 
                                 Description = @Description, Priority = @Priority,
                                 Status = @Status, RequestDate = @RequestDate,
                                 CompletedDate = @CompletedDate, Cost = @Cost,
                                 AssignedTo = @AssignedTo
                                 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", request.Id);
            command.Parameters.AddWithValue("@UnitId", request.UnitId);
            command.Parameters.AddWithValue("@TenantId", request.TenantId);
            command.Parameters.AddWithValue("@Description", request.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Priority", (int)request.Priority);
            command.Parameters.AddWithValue("@Status", (int)request.Status);
            command.Parameters.AddWithValue("@RequestDate", request.RequestDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@CompletedDate", request.CompletedDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Cost", request.Cost.HasValue ? (double)request.Cost.Value : (object)DBNull.Value);
            command.Parameters.AddWithValue("@AssignedTo", request.AssignedTo ?? (object)DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = "DELETE FROM MaintenanceRequests WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }
    }
}