using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ApartmentManagementSystem
{
    public class UnitRepository
    {
        private readonly string _connectionString;

        public UnitRepository()
        {
            _connectionString = DatabaseHelper.GetConnectionString();
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
                    Id = Convert.ToInt32(reader["Id"]),
                    PropertyId = Convert.ToInt32(reader["PropertyId"]),
                    UnitNumber = reader["UnitNumber"].ToString() ?? string.Empty,
                    UnitType = reader["UnitType"]?.ToString() ?? string.Empty,
                    Size = Convert.ToDecimal(reader["Size"] ?? 0),
                    RentAmount = Convert.ToDecimal(reader["RentAmount"] ?? 0),
                    Description = reader["Description"]?.ToString() ?? string.Empty,
                    Status = (UnitStatus)Convert.ToInt32(reader["Status"] ?? 0),
                    Bedrooms = Convert.ToInt32(reader["Bedrooms"] ?? 0),
                    Bathrooms = Convert.ToInt32(reader["Bathrooms"] ?? 0)
                });
            }
            return units;
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
                                 RentAmount, Description, Status, Bedrooms, Bathrooms) 
                                 VALUES (@PropertyId, @UnitNumber, @UnitType, @Size, 
                                 @RentAmount, @Description, @Status, @Bedrooms, @Bathrooms)";

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
                    Id = Convert.ToInt32(reader["Id"]),
                    PropertyId = Convert.ToInt32(reader["PropertyId"]),
                    UnitNumber = reader["UnitNumber"].ToString() ?? string.Empty,
                    UnitType = reader["UnitType"]?.ToString() ?? string.Empty,
                    Size = Convert.ToDecimal(reader["Size"] ?? 0),
                    RentAmount = Convert.ToDecimal(reader["RentAmount"] ?? 0),
                    Description = reader["Description"]?.ToString() ?? string.Empty,
                    Status = (UnitStatus)Convert.ToInt32(reader["Status"] ?? 0),
                    Bedrooms = Convert.ToInt32(reader["Bedrooms"] ?? 0),
                    Bathrooms = Convert.ToInt32(reader["Bathrooms"] ?? 0)
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
                                 Bedrooms = @Bedrooms, Bathrooms = @Bathrooms
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
    }
}