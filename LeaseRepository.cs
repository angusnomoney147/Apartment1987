using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ApartmentManagementSystem
{
    public class LeaseRepository
    {
        private readonly string _connectionString;

        public LeaseRepository()
        {
            _connectionString = "Data Source=apartment.db;Version=3;";
        }

        public List<Lease> GetAll()
        {
            var leases = new List<Lease>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM Leases ORDER BY StartDate DESC", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                leases.Add(new Lease
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    UnitId = Convert.ToInt32(reader["UnitId"]),
                    TenantId = Convert.ToInt32(reader["TenantId"]),
                    StartDate = DateTime.Parse(reader["StartDate"]?.ToString() ?? DateTime.Now.ToString()),
                    EndDate = DateTime.Parse(reader["EndDate"]?.ToString() ?? DateTime.Now.ToString()),
                    MonthlyRent = Convert.ToDecimal(reader["MonthlyRent"] ?? 0),
                    SecurityDeposit = Convert.ToDecimal(reader["SecurityDeposit"] ?? 0),
                    Terms = reader["Terms"]?.ToString() ?? string.Empty,
                    Status = (LeaseStatus)Convert.ToInt32(reader["Status"] ?? 0),
                    CreatedDate = DateTime.Parse(reader["CreatedDate"]?.ToString() ?? DateTime.Now.ToString())
                });
            }
            return leases;
        }

        public void Add(Lease lease)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO Leases (UnitId, TenantId, StartDate, EndDate, 
                                 MonthlyRent, SecurityDeposit, Terms, Status, CreatedDate) 
                                 VALUES (@UnitId, @TenantId, @StartDate, @EndDate, 
                                 @MonthlyRent, @SecurityDeposit, @Terms, @Status, @CreatedDate)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@UnitId", lease.UnitId);
            command.Parameters.AddWithValue("@TenantId", lease.TenantId);
            command.Parameters.AddWithValue("@StartDate", lease.StartDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@EndDate", lease.EndDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@MonthlyRent", lease.MonthlyRent);
            command.Parameters.AddWithValue("@SecurityDeposit", lease.SecurityDeposit);
            command.Parameters.AddWithValue("@Terms", lease.Terms ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)lease.Status);
            command.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd"));
            command.ExecuteNonQuery();
        }

        public void Update(Lease lease)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Leases SET UnitId = @UnitId, TenantId = @TenantId, 
                                 StartDate = @StartDate, EndDate = @EndDate, 
                                 MonthlyRent = @MonthlyRent, SecurityDeposit = @SecurityDeposit,
                                 Terms = @Terms, Status = @Status
                                 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", lease.Id);
            command.Parameters.AddWithValue("@UnitId", lease.UnitId);
            command.Parameters.AddWithValue("@TenantId", lease.TenantId);
            command.Parameters.AddWithValue("@StartDate", lease.StartDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@EndDate", lease.EndDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@MonthlyRent", lease.MonthlyRent);
            command.Parameters.AddWithValue("@SecurityDeposit", lease.SecurityDeposit);
            command.Parameters.AddWithValue("@Terms", lease.Terms ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)lease.Status);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = "DELETE FROM Leases WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }
    }
}