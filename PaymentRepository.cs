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
                    Id = Convert.ToInt32(reader["Id"]),
                    LeaseId = Convert.ToInt32(reader["LeaseId"]),
                    Amount = Convert.ToDecimal(reader["Amount"] ?? 0),
                    PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                    Method = (PaymentMethod)Convert.ToInt32(reader["PaymentMethod"] ?? 0),
                    ReferenceNumber = reader["ReferenceNumber"]?.ToString() ?? string.Empty,
                    Status = (PaymentStatus)Convert.ToInt32(reader["Status"] ?? 0),
                    Notes = reader["Notes"]?.ToString() ?? string.Empty,
                    DueDate = reader["DueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DueDate"]),
                    CreatedDate = reader["CreatedDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["CreatedDate"])
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
                                    Id = Convert.ToInt32(reader["Id"]),
                                    LeaseId = Convert.ToInt32(reader["LeaseId"]),
                                    Amount = Convert.ToDecimal(reader["Amount"] ?? 0),
                                    PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                                    Method = (PaymentMethod)Convert.ToInt32(reader["PaymentMethod"] ?? 0),
                                    ReferenceNumber = reader["ReferenceNumber"]?.ToString() ?? string.Empty,
                                    Status = (PaymentStatus)Convert.ToInt32(reader["Status"] ?? 0),
                                    Notes = reader["Notes"]?.ToString() ?? string.Empty,
                                    DueDate = reader["DueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DueDate"]),
                                    CreatedDate = reader["CreatedDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["CreatedDate"])
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
    }
}