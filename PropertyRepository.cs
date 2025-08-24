using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;

namespace ApartmentManagementSystem
{
    public class PropertyRepository
    {
        private readonly string _connectionString;

        public PropertyRepository()
        {
            _connectionString = "Data Source=apartment.db;Version=3;";
        }

        public List<Property> GetAll()
        {
            var properties = new List<Property>();
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Properties ORDER BY Name", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var property = new Property();

                            property.Id = GetInt32(reader, "Id", 0);
                            property.Name = GetString(reader, "Name", "");
                            property.Address = GetString(reader, "Address", "");
                            property.City = GetString(reader, "City", "");
                            property.State = GetString(reader, "State", "");
                            property.ZipCode = GetString(reader, "ZipCode", "");
                            property.Country = GetString(reader, "Country", "");
                            property.ManagerName = GetString(reader, "ManagerName", "");
                            property.ContactInfo = GetString(reader, "ContactInfo", "");
                            property.TotalUnits = GetInt32(reader, "TotalUnits", 0);
                            property.CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now);

                            properties.Add(property);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}");
            }
            return properties;
        }

        public Property GetById(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT * FROM Properties WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var property = new Property();

                                property.Id = GetInt32(reader, "Id", 0);
                                property.Name = GetString(reader, "Name", "");
                                property.Address = GetString(reader, "Address", "");
                                property.City = GetString(reader, "City", "");
                                property.State = GetString(reader, "State", "");
                                property.ZipCode = GetString(reader, "ZipCode", "");
                                property.Country = GetString(reader, "Country", "");
                                property.ManagerName = GetString(reader, "ManagerName", "");
                                property.ContactInfo = GetString(reader, "ContactInfo", "");
                                property.TotalUnits = GetInt32(reader, "TotalUnits", 0);
                                property.CreatedDate = GetDateTime(reader, "CreatedDate", DateTime.Now);

                                return property;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading property: {ex.Message}");
            }
            return null;
        }

        public void Add(Property property)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO Properties 
                        (Name, Address, City, State, ZipCode, Country, ManagerName, ContactInfo, TotalUnits, CreatedDate)
                        VALUES 
                        (@Name, @Address, @City, @State, @ZipCode, @Country, @ManagerName, @ContactInfo, @TotalUnits, @CreatedDate);
                        SELECT last_insert_rowid();";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", property.Name ?? "");
            command.Parameters.AddWithValue("@Address", property.Address ?? "");
            command.Parameters.AddWithValue("@City", property.City ?? "");
            command.Parameters.AddWithValue("@State", property.State ?? "");
            command.Parameters.AddWithValue("@ZipCode", property.ZipCode ?? "");
            command.Parameters.AddWithValue("@Country", property.Country ?? "");
            command.Parameters.AddWithValue("@ManagerName", property.ManagerName ?? "");
            command.Parameters.AddWithValue("@ContactInfo", property.ContactInfo ?? "");
            command.Parameters.AddWithValue("@TotalUnits", property.TotalUnits);
            command.Parameters.AddWithValue("@CreatedDate", property.CreatedDate);

            var result = command.ExecuteScalar();
            property.Id = Convert.ToInt32(result);
        }

        public void Update(Property property)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(@"
                        UPDATE Properties SET
                        Name = @Name,
                        Address = @Address,
                        City = @City,
                        State = @State,
                        ZipCode = @ZipCode,
                        Country = @Country,
                        ManagerName = @ManagerName,
                        ContactInfo = @ContactInfo,
                        TotalUnits = @TotalUnits,
                        CreatedDate = @CreatedDate
                        WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", property.Id);
                        command.Parameters.AddWithValue("@Name", property.Name ?? "");
                        command.Parameters.AddWithValue("@Address", property.Address ?? "");
                        command.Parameters.AddWithValue("@City", property.City ?? "");
                        command.Parameters.AddWithValue("@State", property.State ?? "");
                        command.Parameters.AddWithValue("@ZipCode", property.ZipCode ?? "");
                        command.Parameters.AddWithValue("@Country", property.Country ?? "");
                        command.Parameters.AddWithValue("@ManagerName", property.ManagerName ?? "");
                        command.Parameters.AddWithValue("@ContactInfo", property.ContactInfo ?? "");
                        command.Parameters.AddWithValue("@TotalUnits", property.TotalUnits);
                        command.Parameters.AddWithValue("@CreatedDate", property.CreatedDate);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating property: {ex.Message}");
            }
        }

        public void Delete(int id)
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("DELETE FROM Properties WHERE Id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting property: {ex.Message}");
            }
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
    }
}