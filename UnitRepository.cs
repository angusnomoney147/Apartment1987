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
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM Units ORDER BY UnitNumber", connection);
            using var reader = command.ExecuteReader();

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
                    Description = GetString(reader, "Description", ""),
                    Status = GetEnum<UnitStatus>(reader, "Status", UnitStatus.Vacant),
                    Bedrooms = GetInt32(reader, "Bedrooms", 0),
                    Bathrooms = GetInt32(reader, "Bathrooms", 0),
                    CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                });
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
                                    Status = GetEnum<UnitStatus>(reader, "Status", UnitStatus.Vacant),
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
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Units SET Status = @Status WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", unitId);
            command.Parameters.AddWithValue("@Status", (int)newStatus);
            command.ExecuteNonQuery();
        }

        public void Add(Unit unit)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO Units (PropertyId, UnitNumber, UnitType, Size, 
                                 RentAmount, Description, Status, Bedrooms, Bathrooms, CreatedDate) 
                                 VALUES (@PropertyId, @UnitNumber, @UnitType, @Size, 
                                 @RentAmount, @Description, @Status, @Bedrooms, @Bathrooms, @CreatedDate)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@PropertyId", unit.PropertyId);
            command.Parameters.AddWithValue("@UnitNumber", unit.UnitNumber);
            command.Parameters.AddWithValue("@UnitType", unit.UnitType ?? string.Empty);
            command.Parameters.AddWithValue("@Size", unit.Size);
            command.Parameters.AddWithValue("@RentAmount", unit.RentAmount);
            command.Parameters.AddWithValue("@Description", unit.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)unit.Status);
            command.Parameters.AddWithValue("@Bedrooms", unit.Bedrooms);
            command.Parameters.AddWithValue("@Bathrooms", unit.Bathrooms);
            command.Parameters.AddWithValue("@CreatedDate", unit.CreatedDate);
            command.ExecuteNonQuery();
        }

        public List<Unit> GetUnitsByProperty(int propertyId)
        {
            var units = new List<Unit>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM Units WHERE PropertyId = @PropertyId ORDER BY UnitNumber", connection);
            command.Parameters.AddWithValue("@PropertyId", propertyId);
            using var reader = command.ExecuteReader();

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
                    Description = GetString(reader, "Description", ""),
                    Status = GetEnum<UnitStatus>(reader, "Status", UnitStatus.Vacant),
                    Bedrooms = GetInt32(reader, "Bedrooms", 0),
                    Bathrooms = GetInt32(reader, "Bathrooms", 0),
                    CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now)
                });
            }
            return units;
        }

        public void Update(Unit unit)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Units SET PropertyId = @PropertyId, UnitNumber = @UnitNumber, 
                                 UnitType = @UnitType, Size = @Size, RentAmount = @RentAmount,
                                 Description = @Description, Status = @Status, 
                                 Bedrooms = @Bedrooms, Bathrooms = @Bathrooms, CreatedDate = @CreatedDate
                                 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", unit.Id);
            command.Parameters.AddWithValue("@PropertyId", unit.PropertyId);
            command.Parameters.AddWithValue("@UnitNumber", unit.UnitNumber);
            command.Parameters.AddWithValue("@UnitType", unit.UnitType ?? string.Empty);
            command.Parameters.AddWithValue("@Size", unit.Size);
            command.Parameters.AddWithValue("@RentAmount", unit.RentAmount);
            command.Parameters.AddWithValue("@Description", unit.Description ?? string.Empty);
            command.Parameters.AddWithValue("@Status", (int)unit.Status);
            command.Parameters.AddWithValue("@Bedrooms", unit.Bedrooms);
            command.Parameters.AddWithValue("@Bathrooms", unit.Bathrooms);
            command.Parameters.AddWithValue("@CreatedDate", unit.CreatedDate);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = "DELETE FROM Units WHERE Id = @Id";

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