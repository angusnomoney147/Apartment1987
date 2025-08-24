using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

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
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Leases ORDER BY StartDate DESC", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var lease = new Lease();

                            lease.Id = GetInt32(reader, "Id", 0);
                            lease.TenantId = GetInt32(reader, "TenantId", 0);
                            lease.UnitId = GetInt32(reader, "UnitId", 0);
                            lease.StartDate = GetDateTime(reader, "StartDate", DateTime.Now);
                            lease.EndDate = GetDateTime(reader, "EndDate", DateTime.Now);
                            lease.MonthlyRent = GetDecimal(reader, "MonthlyRent", 0);
                            lease.SecurityDeposit = GetDecimal(reader, "SecurityDeposit", 0);
                            lease.Terms = GetString(reader, "Terms", "");
                            lease.CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now);
                            lease.Status = GetEnum<LeaseStatus>(reader, "Status", LeaseStatus.Active);

                            leases.Add(lease);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading leases: {ex.Message}");
            }
            return leases;
        }

        public Lease GetById(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Leases WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Lease
                                {
                                    Id = GetInt32(reader, "Id", 0),
                                    TenantId = GetInt32(reader, "TenantId", 0),
                                    UnitId = GetInt32(reader, "UnitId", 0),
                                    StartDate = GetDateTime(reader, "StartDate", DateTime.Now),
                                    EndDate = GetDateTime(reader, "EndDate", DateTime.Now),
                                    MonthlyRent = GetDecimal(reader, "MonthlyRent", 0),
                                    SecurityDeposit = GetDecimal(reader, "SecurityDeposit", 0),
                                    Terms = GetString(reader, "Terms", ""),
                                    Status = GetEnum<LeaseStatus>(reader, "Status", LeaseStatus.Active),
                                    CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading lease: {ex.Message}");
            }
            return null;
        }

        public void Add(Lease lease)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO Leases 
                                (TenantId, UnitId, StartDate, EndDate, MonthlyRent, SecurityDeposit, Terms, Status, CreatedDate)
                                VALUES 
                                (@TenantId, @UnitId, @StartDate, @EndDate, @MonthlyRent, @SecurityDeposit, @Terms, @Status, @CreatedDate);
                                SELECT last_insert_rowid();";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@TenantId", lease.TenantId);
            command.Parameters.AddWithValue("@UnitId", lease.UnitId);
            command.Parameters.AddWithValue("@StartDate", lease.StartDate);
            command.Parameters.AddWithValue("@EndDate", lease.EndDate);
            command.Parameters.AddWithValue("@MonthlyRent", lease.MonthlyRent);
            command.Parameters.AddWithValue("@SecurityDeposit", lease.SecurityDeposit);
            command.Parameters.AddWithValue("@Terms", lease.Terms ?? "");
            command.Parameters.AddWithValue("@Status", lease.Status.ToString());
            command.Parameters.AddWithValue("@CreatedDate", lease.CreatedDate);

            var result = command.ExecuteScalar();
            lease.Id = Convert.ToInt32(result);
        }

        public void Update(Lease lease)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Leases SET TenantId = @TenantId, UnitId = @UnitId, 
                                 StartDate = @StartDate, EndDate = @EndDate, MonthlyRent = @MonthlyRent,
                                 SecurityDeposit = @SecurityDeposit, Terms = @Terms, Status = @Status
                                 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", lease.Id);
            command.Parameters.AddWithValue("@TenantId", lease.TenantId);
            command.Parameters.AddWithValue("@UnitId", lease.UnitId);
            command.Parameters.AddWithValue("@StartDate", lease.StartDate);
            command.Parameters.AddWithValue("@EndDate", lease.EndDate);
            command.Parameters.AddWithValue("@MonthlyRent", lease.MonthlyRent);
            command.Parameters.AddWithValue("@SecurityDeposit", lease.SecurityDeposit);
            command.Parameters.AddWithValue("@Terms", lease.Terms ?? "");
            command.Parameters.AddWithValue("@Status", lease.Status.ToString());
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

        private decimal GetDecimal(SQLiteDataReader reader, string columnName, decimal defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return !reader.IsDBNull(ordinal) ? reader.GetDecimal(ordinal) : defaultValue;
            }
            catch
            {
                return defaultValue;
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
                    else if (int.TryParse(value, out int intValue))
                    {
                        return (T)Enum.ToObject(typeof(T), intValue);
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