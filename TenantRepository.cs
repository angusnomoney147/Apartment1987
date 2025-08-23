using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;

namespace ApartmentManagementSystem
{
    public class TenantRepository
    {
        private readonly string _connectionString;

        public TenantRepository()
        {
            _connectionString = "Data Source=apartment.db;Version=3;";
        }

        public List<Tenant> GetAll()
        {
            var tenants = new List<Tenant>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Tenants ORDER BY LastName, FirstName", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tenant = new Tenant();

                            tenant.Id = GetInt32(reader, "Id", 0);
                            tenant.FirstName = GetString(reader, "FirstName", "");
                            tenant.LastName = GetString(reader, "LastName", "");
                            tenant.Email = GetString(reader, "Email", "");
                            tenant.Phone = GetString(reader, "Phone", "");
                            tenant.UnitNumber = GetString(reader, "UnitNumber", "");
                            tenant.PropertyName = GetString(reader, "PropertyName", "");
                            tenant.IsActive = GetInt32(reader, "IsActive", 1) == 1;
                            tenant.NationalId = GetString(reader, "NationalId", "");
                            tenant.Address = GetString(reader, "Address", "");
                            tenant.EmergencyContact = GetString(reader, "EmergencyContact", "");
                            tenant.EmergencyPhone = GetString(reader, "EmergencyPhone", "");

                            // Handle nullable dates
                            tenant.LeaseStartDate = GetNullableDateTime(reader, "LeaseStartDate");
                            tenant.LeaseEndDate = GetNullableDateTime(reader, "LeaseEndDate");
                            tenant.DateOfBirth = GetNullableDateTime(reader, "DateOfBirth");
                            tenant.CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now);

                            // Handle nullable decimal
                            tenant.RentAmount = GetNullableDecimal(reader, "RentAmount");

                            tenants.Add(tenant);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tenants: {ex.Message}");
            }
            return tenants;
        }

        public Tenant GetById(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Tenants WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var tenant = new Tenant();

                                tenant.Id = GetInt32(reader, "Id", 0);
                                tenant.FirstName = GetString(reader, "FirstName", "");
                                tenant.LastName = GetString(reader, "LastName", "");
                                tenant.Email = GetString(reader, "Email", "");
                                tenant.Phone = GetString(reader, "Phone", "");
                                tenant.UnitNumber = GetString(reader, "UnitNumber", "");
                                tenant.PropertyName = GetString(reader, "PropertyName", "");
                                tenant.IsActive = GetInt32(reader, "IsActive", 1) == 1;
                                tenant.NationalId = GetString(reader, "NationalId", "");
                                tenant.Address = GetString(reader, "Address", "");
                                tenant.EmergencyContact = GetString(reader, "EmergencyContact", "");
                                tenant.EmergencyPhone = GetString(reader, "EmergencyPhone", "");

                                tenant.LeaseStartDate = GetNullableDateTime(reader, "LeaseStartDate");
                                tenant.LeaseEndDate = GetNullableDateTime(reader, "LeaseEndDate");
                                tenant.DateOfBirth = GetNullableDateTime(reader, "DateOfBirth");
                                tenant.CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now);
                                tenant.RentAmount = GetNullableDecimal(reader, "RentAmount");

                                return tenant;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tenant: {ex.Message}");
            }
            return null;
        }

        public void Add(Tenant tenant)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(@"
                        INSERT INTO Tenants 
                        (FirstName, LastName, Email, Phone, UnitNumber, PropertyName, IsActive, LeaseStartDate, LeaseEndDate, RentAmount, CreatedDate, DateOfBirth, NationalId, Address, EmergencyContact, EmergencyPhone)
                        VALUES 
                        (@FirstName, @LastName, @Email, @Phone, @UnitNumber, @PropertyName, @IsActive, @LeaseStartDate, @LeaseEndDate, @RentAmount, @CreatedDate, @DateOfBirth, @NationalId, @Address, @EmergencyContact, @EmergencyPhone)", connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", tenant.FirstName ?? "");
                        command.Parameters.AddWithValue("@LastName", tenant.LastName ?? "");
                        command.Parameters.AddWithValue("@Email", tenant.Email ?? "");
                        command.Parameters.AddWithValue("@Phone", tenant.Phone ?? "");
                        command.Parameters.AddWithValue("@UnitNumber", tenant.UnitNumber ?? "");
                        command.Parameters.AddWithValue("@PropertyName", tenant.PropertyName ?? "");
                        command.Parameters.AddWithValue("@IsActive", tenant.IsActive ? 1 : 0);
                        command.Parameters.AddWithValue("@NationalId", tenant.NationalId ?? "");
                        command.Parameters.AddWithValue("@Address", tenant.Address ?? "");
                        command.Parameters.AddWithValue("@EmergencyContact", tenant.EmergencyContact ?? "");
                        command.Parameters.AddWithValue("@EmergencyPhone", tenant.EmergencyPhone ?? "");
                        command.Parameters.AddWithValue("@LeaseStartDate", (object)tenant.LeaseStartDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LeaseEndDate", (object)tenant.LeaseEndDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DateOfBirth", (object)tenant.DateOfBirth ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RentAmount", (object)tenant.RentAmount ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDate", tenant.CreatedDate);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding tenant: {ex.Message}");
            }
        }

        public void Update(Tenant tenant)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(@"
                        UPDATE Tenants SET
                        FirstName = @FirstName,
                        LastName = @LastName,
                        Email = @Email,
                        Phone = @Phone,
                        UnitNumber = @UnitNumber,
                        PropertyName = @PropertyName,
                        IsActive = @IsActive,
                        LeaseStartDate = @LeaseStartDate,
                        LeaseEndDate = @LeaseEndDate,
                        RentAmount = @RentAmount,
                        CreatedDate = @CreatedDate,
                        DateOfBirth = @DateOfBirth,
                        NationalId = @NationalId,
                        Address = @Address,
                        EmergencyContact = @EmergencyContact,
                        EmergencyPhone = @EmergencyPhone
                        WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", tenant.Id);
                        command.Parameters.AddWithValue("@FirstName", tenant.FirstName ?? "");
                        command.Parameters.AddWithValue("@LastName", tenant.LastName ?? "");
                        command.Parameters.AddWithValue("@Email", tenant.Email ?? "");
                        command.Parameters.AddWithValue("@Phone", tenant.Phone ?? "");
                        command.Parameters.AddWithValue("@UnitNumber", tenant.UnitNumber ?? "");
                        command.Parameters.AddWithValue("@PropertyName", tenant.PropertyName ?? "");
                        command.Parameters.AddWithValue("@IsActive", tenant.IsActive ? 1 : 0);
                        command.Parameters.AddWithValue("@NationalId", tenant.NationalId ?? "");
                        command.Parameters.AddWithValue("@Address", tenant.Address ?? "");
                        command.Parameters.AddWithValue("@EmergencyContact", tenant.EmergencyContact ?? "");
                        command.Parameters.AddWithValue("@EmergencyPhone", tenant.EmergencyPhone ?? "");
                        command.Parameters.AddWithValue("@LeaseStartDate", (object)tenant.LeaseStartDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LeaseEndDate", (object)tenant.LeaseEndDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DateOfBirth", (object)tenant.DateOfBirth ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RentAmount", (object)tenant.RentAmount ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDate", tenant.CreatedDate);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating tenant: {ex.Message}");
            }
        }

        public void Delete(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("DELETE FROM Tenants WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting tenant: {ex.Message}");
            }
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
    }
}