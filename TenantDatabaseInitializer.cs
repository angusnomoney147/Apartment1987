using System;
using System.Data.SQLite;
using System.Windows;

namespace ApartmentManagementSystem
{
    public static class TenantDatabaseInitializer
    {
        private const string ConnectionString = "Data Source=apartment.db;Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                // Create database file if it doesn't exist
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Check if Tenants table exists
                    bool tableExists = TableExists(connection, "Tenants");

                    if (!tableExists)
                    {
                        // Create new table with all columns
                        CreateTenantsTable(connection);
                    }
                    else
                    {
                        // Add missing columns if they don't exist
                        AddMissingColumns(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing tenant database: {ex.Message}");
            }
        }

        private static bool TableExists(SQLiteConnection connection, string tableName)
        {
            try
            {
                using (var command = new SQLiteCommand(@"
                    SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName", connection))
                {
                    command.Parameters.AddWithValue("@tableName", tableName);
                    return command.ExecuteScalar() != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void CreateTenantsTable(SQLiteConnection connection)
        {
            try
            {
                using (var command = new SQLiteCommand(@"
                    CREATE TABLE Tenants (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FirstName TEXT NOT NULL,
                        LastName TEXT NOT NULL,
                        Email TEXT,
                        Phone TEXT,
                        UnitNumber TEXT,
                        PropertyName TEXT,
                        IsActive INTEGER NOT NULL DEFAULT 1,
                        LeaseStartDate DATETIME,
                        LeaseEndDate DATETIME,
                        RentAmount DECIMAL,
                        CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        DateOfBirth DATETIME,
                        NationalId TEXT,
                        Address TEXT,
                        EmergencyContact TEXT,
                        EmergencyPhone TEXT
                    )", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tenants table: {ex.Message}");
            }
        }

        private static void AddMissingColumns(SQLiteConnection connection)
        {
            try
            {
                // Get existing columns
                var existingColumns = new System.Collections.Generic.List<string>();
                using (var command = new SQLiteCommand("PRAGMA table_info(Tenants)", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingColumns.Add(reader["name"].ToString().ToLower());
                    }
                }

                // Add missing columns
                var columnsToAdd = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "dateofbirth", "ALTER TABLE Tenants ADD COLUMN DateOfBirth DATETIME" },
                    { "nationalid", "ALTER TABLE Tenants ADD COLUMN NationalId TEXT" },
                    { "address", "ALTER TABLE Tenants ADD COLUMN Address TEXT" },
                    { "emergencycontact", "ALTER TABLE Tenants ADD COLUMN EmergencyContact TEXT" },
                    { "emergencyphone", "ALTER TABLE Tenants ADD COLUMN EmergencyPhone TEXT" }
                };

                foreach (var column in columnsToAdd)
                {
                    if (!existingColumns.Contains(column.Key))
                    {
                        try
                        {
                            using (var command = new SQLiteCommand(column.Value, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            // Column might already exist or other issue, continue
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding missing columns: {ex.Message}");
            }
        }
    }
}