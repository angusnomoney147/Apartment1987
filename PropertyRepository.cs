using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ApartmentManagementSystem
{
    public class PropertyRepository
    {
        private readonly string _connectionString;

        public PropertyRepository()
        {
            _connectionString = DatabaseHelper.GetConnectionString();
        }

        public List<Property> GetAll()
        {
            var properties = new List<Property>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM Properties ORDER BY Name", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                properties.Add(new Property
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString() ?? string.Empty,
                    Address = reader["Address"]?.ToString() ?? string.Empty,
                    City = reader["City"]?.ToString() ?? string.Empty,
                    Country = reader["Country"]?.ToString() ?? string.Empty,
                    TotalUnits = Convert.ToInt32(reader["TotalUnits"]),
                    ManagerName = reader["ManagerName"]?.ToString() ?? string.Empty,
                    ContactInfo = reader["ContactInfo"]?.ToString() ?? string.Empty,
                    CreatedDate = DateTime.Parse(reader["CreatedDate"]?.ToString() ?? DateTime.Now.ToString())
                });
            }
            return properties;
        }

        // KEEP ONLY ONE Add method that returns the ID
        public int Add(Property property)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO Properties (Name, Address, City, Country, 
                                 TotalUnits, ManagerName, ContactInfo, CreatedDate) 
                                 VALUES (@Name, @Address, @City, @Country, 
                                 @TotalUnits, @ManagerName, @ContactInfo, @CreatedDate);
                                 SELECT last_insert_rowid();";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", property.Name);
            command.Parameters.AddWithValue("@Address", property.Address ?? string.Empty);
            command.Parameters.AddWithValue("@City", property.City ?? string.Empty);
            command.Parameters.AddWithValue("@Country", property.Country ?? string.Empty);
            command.Parameters.AddWithValue("@TotalUnits", property.TotalUnits);
            command.Parameters.AddWithValue("@ManagerName", property.ManagerName ?? string.Empty);
            command.Parameters.AddWithValue("@ContactInfo", property.ContactInfo ?? string.Empty);
            command.Parameters.AddWithValue("@CreatedDate", property.CreatedDate.ToString("yyyy-MM-dd"));

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        public void Update(Property property)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Properties SET Name = @Name, Address = @Address, 
                                 City = @City, Country = @Country, TotalUnits = @TotalUnits,
                                 ManagerName = @ManagerName, ContactInfo = @ContactInfo
                                 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", property.Id);
            command.Parameters.AddWithValue("@Name", property.Name);
            command.Parameters.AddWithValue("@Address", property.Address ?? string.Empty);
            command.Parameters.AddWithValue("@City", property.City ?? string.Empty);
            command.Parameters.AddWithValue("@Country", property.Country ?? string.Empty);
            command.Parameters.AddWithValue("@TotalUnits", property.TotalUnits);
            command.Parameters.AddWithValue("@ManagerName", property.ManagerName ?? string.Empty);
            command.Parameters.AddWithValue("@ContactInfo", property.ContactInfo ?? string.Empty);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = "DELETE FROM Properties WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }
    }
}