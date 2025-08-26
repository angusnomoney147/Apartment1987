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
                try
                {
                    var payment = new Payment
                    {
                        Id = GetInt32(reader, "Id", 0),
                        LeaseId = GetInt32(reader, "LeaseId", 0),
                        Amount = GetDecimal(reader, "Amount", 0),
                        PaymentDate = GetDateTime(reader, "PaymentDate", DateTime.Now),
                        Method = GetPaymentMethod(reader, "PaymentMethod", PaymentMethod.Other),
                        ReferenceNumber = GetString(reader, "ReferenceNumber", ""),
                        Status = GetPaymentStatus(reader, "Status", PaymentStatus.Pending),
                        Notes = GetString(reader, "Notes", ""),
                        CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                    };
                    payments.Add(payment);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading payment: {ex.Message}");
                }
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
                                    Method = GetPaymentMethod(reader, "PaymentMethod", PaymentMethod.Other),
                                    ReferenceNumber = GetString(reader, "ReferenceNumber", ""),
                                    Status = GetPaymentStatus(reader, "Status", PaymentStatus.Pending),
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

            const string query = @"INSERT INTO Payments 
                        (LeaseId, Amount, PaymentDate, PaymentMethod, ReferenceNumber, Status, Notes, CreatedDate)
                        VALUES 
                        (@LeaseId, @Amount, @PaymentDate, @PaymentMethod, @ReferenceNumber, @Status, @Notes, @CreatedDate);
                        SELECT last_insert_rowid();";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@LeaseId", payment.LeaseId);
            command.Parameters.AddWithValue("@Amount", payment.Amount);
            command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
            command.Parameters.AddWithValue("@PaymentMethod", (int)payment.Method); // Cast to int
            command.Parameters.AddWithValue("@ReferenceNumber", payment.ReferenceNumber ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)payment.Status); // Cast to int
            command.Parameters.AddWithValue("@Notes", payment.Notes ?? string.Empty);
            command.Parameters.AddWithValue("@CreatedDate", payment.CreatedDate);

            var result = command.ExecuteScalar();
            payment.Id = Convert.ToInt32(result);
        }

        public void Update(Payment payment)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Payments SET 
                         LeaseId = @LeaseId, Amount = @Amount, PaymentDate = @PaymentDate,
                         PaymentMethod = @PaymentMethod, ReferenceNumber = @ReferenceNumber,
                         Status = @Status, Notes = @Notes
                         WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", payment.Id);
            command.Parameters.AddWithValue("@LeaseId", payment.LeaseId);
            command.Parameters.AddWithValue("@Amount", payment.Amount);
            command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
            command.Parameters.AddWithValue("@PaymentMethod", (int)payment.Method); // Cast enum to int
            command.Parameters.AddWithValue("@ReferenceNumber", payment.ReferenceNumber ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)payment.Status); // Cast enum to int
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

        // Helper methods
        private int GetInt32(SQLiteDataReader reader, string columnName, int defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (!reader.IsDBNull(ordinal))
                {
                    var value = reader.GetValue(ordinal);
                    if (value is long longValue)
                    {
                        return (int)longValue;
                    }
                    else if (value is int intValue)
                    {
                        return intValue;
                    }
                    else if (value is string stringValue && int.TryParse(stringValue, out int parsedValue))
                    {
                        return parsedValue;
                    }
                }
                return defaultValue;
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
                if (!reader.IsDBNull(ordinal))
                {
                    var value = reader.GetValue(ordinal);
                    if (value is DateTime dateTime)
                    {
                        return dateTime;
                    }
                    else if (value is string dateString && DateTime.TryParse(dateString, out DateTime parsedDate))
                    {
                        return parsedDate;
                    }
                }
                return defaultValue;
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
                if (!reader.IsDBNull(ordinal))
                {
                    var value = reader.GetValue(ordinal);
                    if (value is decimal decimalValue)
                    {
                        return decimalValue;
                    }
                    else if (value is double doubleValue)
                    {
                        return (decimal)doubleValue;
                    }
                    else if (value is long longValue)
                    {
                        return (decimal)longValue;
                    }
                    else if (value is int intValue)
                    {
                        return (decimal)intValue;
                    }
                    else if (value is string stringValue && decimal.TryParse(stringValue, out decimal parsedValue))
                    {
                        return parsedValue;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private PaymentMethod GetPaymentMethod(SQLiteDataReader reader, string columnName, PaymentMethod defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (!reader.IsDBNull(ordinal))
                {
                    var value = reader.GetValue(ordinal);
                    int intValue;

                    if (value is long longValue)
                    {
                        intValue = (int)longValue;
                    }
                    else if (value is int intValue2)
                    {
                        intValue = intValue2;
                    }
                    else if (value is string stringValue && int.TryParse(stringValue, out int parsedValue))
                    {
                        intValue = parsedValue;
                    }
                    else
                    {
                        return defaultValue;
                    }

                    if (Enum.IsDefined(typeof(PaymentMethod), intValue))
                    {
                        return (PaymentMethod)intValue;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private PaymentStatus GetPaymentStatus(SQLiteDataReader reader, string columnName, PaymentStatus defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (!reader.IsDBNull(ordinal))
                {
                    var value = reader.GetValue(ordinal);
                    int intValue;

                    // Handle different data types
                    if (value is long longValue)
                    {
                        intValue = (int)longValue;
                    }
                    else if (value is int intValue2)
                    {
                        intValue = intValue2;
                    }
                    else if (value is string stringValue && int.TryParse(stringValue, out int parsedValue))
                    {
                        intValue = parsedValue;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Invalid PaymentStatus value type: {value.GetType()} with value: {value}");
                        return defaultValue;
                    }

                    if (Enum.IsDefined(typeof(PaymentStatus), intValue))
                    {
                        return (PaymentStatus)intValue;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Undefined PaymentStatus value: {intValue}");
                        // Try mapping common values
                        return MapPaymentStatusValue(intValue, defaultValue);
                    }
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading PaymentStatus: {ex.Message}");
                return defaultValue;
            }
        }

        private PaymentStatus MapPaymentStatusValue(int value, PaymentStatus defaultValue)
        {
            // Common mappings for PaymentStatus
            return value switch
            {
                0 => PaymentStatus.Pending,
                1 => PaymentStatus.Completed, // This should be "Paid"
                2 => PaymentStatus.Overdue,
                3 => PaymentStatus.Cancelled,
                _ => defaultValue
            };
        }
    }
}