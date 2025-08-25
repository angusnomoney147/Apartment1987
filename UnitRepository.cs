using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

namespace ApartmentManagementSystem
{
    public class UnitRepository
    {
        private readonly string _connectionString;

        public UnitRepository()
        {
            _connectionString = "Data Source=apartment.db;Version=3;";
        }

        public List<Unit> GetAll()
        {
            var units = new List<Unit>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Units ORDER BY UnitNumber", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            units.Add(new Unit
                            {
                                Id = GetInt32(reader, "Id", 0),
                                PropertyId = GetInt32(reader, "PropertyId", 0),
                                UnitNumber = GetString(reader, "UnitNumber", ""),
                                UnitType = GetString(reader, "UnitType", ""),
                                Size = GetDecimal(reader, "Size", 0),
                                RentAmount = GetDecimal(reader, "RentAmount", 0),
                                Bedrooms = GetInt32(reader, "Bedrooms", 0),
                                Bathrooms = GetInt32(reader, "Bathrooms", 0),
                                Description = GetString(reader, "Description", ""),
                                Status = GetUnitStatus(reader, "Status", UnitStatus.Vacant),
                                CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading units: {ex.Message}");
            }
            return units;
        }

        public Unit GetById(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Units WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Unit
                                {
                                    Id = GetInt32(reader, "Id", 0),
                                    PropertyId = GetInt32(reader, "PropertyId", 0),
                                    UnitNumber = GetString(reader, "UnitNumber", ""),
                                    UnitType = GetString(reader, "UnitType", ""),
                                    Size = GetDecimal(reader, "Size", 0),
                                    RentAmount = GetDecimal(reader, "RentAmount", 0),
                                    Bedrooms = GetInt32(reader, "Bedrooms", 0),
                                    Bathrooms = GetInt32(reader, "Bathrooms", 0),
                                    Description = GetString(reader, "Description", ""),
                                    Status = GetUnitStatus(reader, "Status", UnitStatus.Vacant),
                                    CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading unit: {ex.Message}");
            }
            return null;
        }

        public void UpdateUnitStatus(int unitId, UnitStatus newStatus)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("UPDATE Units SET Status = @Status WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", unitId);
                        command.Parameters.AddWithValue("@Status", (int)newStatus);
                        int rowsAffected = command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows for unit {unitId} to status {(int)newStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateUnitStatus: {ex.Message}");
                MessageBox.Show($"Error updating unit status: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Add(Unit unit)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(@"
                        INSERT INTO Units (PropertyId, UnitNumber, UnitType, Size, 
                                         RentAmount, Description, Status, Bedrooms, Bathrooms, CreatedDate) 
                        VALUES (@PropertyId, @UnitNumber, @UnitType, @Size, 
                                @RentAmount, @Description, @Status, @Bedrooms, @Bathrooms, @CreatedDate)", connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", unit.PropertyId);
                        command.Parameters.AddWithValue("@UnitNumber", unit.UnitNumber ?? "");
                        command.Parameters.AddWithValue("@UnitType", unit.UnitType ?? "");
                        command.Parameters.AddWithValue("@Size", unit.Size);
                        command.Parameters.AddWithValue("@RentAmount", unit.RentAmount);
                        command.Parameters.AddWithValue("@Description", unit.Description ?? "");
                        command.Parameters.AddWithValue("@Status", (int)unit.Status);
                        command.Parameters.AddWithValue("@Bedrooms", unit.Bedrooms);
                        command.Parameters.AddWithValue("@Bathrooms", unit.Bathrooms);
                        command.Parameters.AddWithValue("@CreatedDate", unit.CreatedDate);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding unit: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<Unit> GetUnitsByProperty(int propertyId)
        {
            var units = new List<Unit>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Units WHERE PropertyId = @PropertyId ORDER BY UnitNumber", connection))
                    {
                        command.Parameters.AddWithValue("@PropertyId", propertyId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                units.Add(new Unit
                                {
                                    Id = GetInt32(reader, "Id", 0),
                                    PropertyId = GetInt32(reader, "PropertyId", 0),
                                    UnitNumber = GetString(reader, "UnitNumber", ""),
                                    UnitType = GetString(reader, "UnitType", ""),
                                    Size = GetDecimal(reader, "Size", 0),
                                    RentAmount = GetDecimal(reader, "RentAmount", 0),
                                    Bedrooms = GetInt32(reader, "Bedrooms", 0),
                                    Bathrooms = GetInt32(reader, "Bathrooms", 0),
                                    Description = GetString(reader, "Description", ""),
                                    Status = GetUnitStatus(reader, "Status", UnitStatus.Vacant),
                                    CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading units: {ex.Message}");
            }
            return units;
        }

        public void Update(Unit unit)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(@"
                        UPDATE Units SET PropertyId = @PropertyId, UnitNumber = @UnitNumber, 
                                       UnitType = @UnitType, Size = @Size, RentAmount = @RentAmount,
                                       Description = @Description, Status = @Status, 
                                       Bedrooms = @Bedrooms, Bathrooms = @Bathrooms, CreatedDate = @CreatedDate
                        WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", unit.Id);
                        command.Parameters.AddWithValue("@PropertyId", unit.PropertyId);
                        command.Parameters.AddWithValue("@UnitNumber", unit.UnitNumber ?? "");
                        command.Parameters.AddWithValue("@UnitType", unit.UnitType ?? "");
                        command.Parameters.AddWithValue("@Size", unit.Size);
                        command.Parameters.AddWithValue("@RentAmount", unit.RentAmount);
                        command.Parameters.AddWithValue("@Description", unit.Description ?? "");
                        command.Parameters.AddWithValue("@Status", (int)unit.Status);
                        command.Parameters.AddWithValue("@Bedrooms", unit.Bedrooms);
                        command.Parameters.AddWithValue("@Bathrooms", unit.Bathrooms);
                        command.Parameters.AddWithValue("@CreatedDate", unit.CreatedDate);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating unit: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Delete(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("DELETE FROM Units WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting unit: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper methods
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

        private UnitStatus GetUnitStatus(SQLiteDataReader reader, string columnName, UnitStatus defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (!reader.IsDBNull(ordinal))
                {
                    var value = reader.GetInt32(ordinal);
                    if (Enum.IsDefined(typeof(UnitStatus), value))
                    {
                        return (UnitStatus)value;
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