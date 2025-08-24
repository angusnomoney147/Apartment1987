using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

namespace ApartmentManagementSystem
{
    public class PaymentRepository
    {
        private readonly string _connectionString;

        public PaymentRepository()
        {
            _connectionString = "Data Source=apartment.db;Version=3;";
        }

        public List<Payment> GetAll()
        {
            var payments = new List<Payment>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM Payments ORDER BY PaymentDate DESC", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                payments.Add(new Payment
                {
                    Id = GetInt32(reader, "Id", 0),
                    LeaseId = GetInt32(reader, "LeaseId", 0),
                    Amount = GetDecimal(reader, "Amount", 0),
                    PaymentDate = GetDateTime(reader, "PaymentDate", DateTime.Now),
                    DueDate = GetNullableDateTime(reader, "DueDate"),
                    Status = GetEnum<PaymentStatus>(reader, "Status", PaymentStatus.Pending),
                    Method = GetEnum<PaymentMethod>(reader, "PaymentMethod", PaymentMethod.Other),
                    ReferenceNumber = GetString(reader, "ReferenceNumber", ""),
                    Notes = GetString(reader, "Notes", ""),
                    CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                });
            }
            return payments;
        }

        public Payment GetById(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Payments WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Payment
                                {
                                    Id = GetInt32(reader, "Id", 0),
                                    LeaseId = GetInt32(reader, "LeaseId", 0),
                                    Amount = GetDecimal(reader, "Amount", 0),
                                    PaymentDate = GetDateTime(reader, "PaymentDate", DateTime.Now),
                                    DueDate = GetNullableDateTime(reader, "DueDate"),
                                    Status = GetEnum<PaymentStatus>(reader, "Status", PaymentStatus.Pending),
                                    Method = GetEnum<PaymentMethod>(reader, "PaymentMethod", PaymentMethod.Other),
                                    ReferenceNumber = GetString(reader, "ReferenceNumber", ""),
                                    Notes = GetString(reader, "Notes", ""),
                                    CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment: {ex.Message}");
            }
            return null;
        }

        public void Add(Payment payment)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO Payments (LeaseId, Amount, PaymentDate, 
                                 PaymentMethod, ReferenceNumber, Status, Notes, CreatedDate) 
                                 VALUES (@LeaseId, @Amount, @PaymentDate, 
                                 @PaymentMethod, @ReferenceNumber, @Status, @Notes, @CreatedDate)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@LeaseId", payment.LeaseId);
            command.Parameters.AddWithValue("@Amount", payment.Amount);
            command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
            command.Parameters.AddWithValue("@PaymentMethod", (int)payment.Method);
            command.Parameters.AddWithValue("@ReferenceNumber", payment.ReferenceNumber ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)payment.Status);
            command.Parameters.AddWithValue("@Notes", payment.Notes ?? string.Empty);
            command.Parameters.AddWithValue("@CreatedDate", payment.CreatedDate);
            command.ExecuteNonQuery();
        }

        public void Update(Payment payment)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Payments SET LeaseId = @LeaseId, Amount = @Amount, 
                                 PaymentDate = @PaymentDate, PaymentMethod = @PaymentMethod,
                                 ReferenceNumber = @ReferenceNumber, Status = @Status, Notes = @Notes
                                 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", payment.Id);
            command.Parameters.AddWithValue("@LeaseId", payment.LeaseId);
            command.Parameters.AddWithValue("@Amount", payment.Amount);
            command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
            command.Parameters.AddWithValue("@PaymentMethod", (int)payment.Method);
            command.Parameters.AddWithValue("@ReferenceNumber", payment.ReferenceNumber ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)payment.Status);
            command.Parameters.AddWithValue("@Notes", payment.Notes ?? string.Empty);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = "DELETE FROM Payments WHERE Id = @Id";

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