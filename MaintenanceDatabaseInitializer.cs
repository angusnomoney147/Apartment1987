using System;
using System.Data.SQLite;
using System.Windows;

namespace ApartmentManagementSystem
{
    public static class MaintenanceDatabaseInitializer
    {
        private const string ConnectionString = "Data Source=apartment.db;Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                // Create the database file if it doesn't exist
                SQLiteConnection.CreateFile("apartment.db");
            }
            catch
            {
                // File already exists, that's fine
            }

            try
            {
                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();

                    // Check if MaintenanceRequests table exists
                    var checkTableCommand = new SQLiteCommand(@"
                        SELECT name FROM sqlite_master WHERE type='table' AND name='MaintenanceRequests'", connection);

                    var result = checkTableCommand.ExecuteScalar();

                    if (result == null)
                    {
                        // Create MaintenanceRequests table
                        var createTableCommand = new SQLiteCommand(@"
                            CREATE TABLE MaintenanceRequests (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                UnitId INTEGER NOT NULL,
                                TenantId INTEGER NOT NULL,
                                UnitNumber TEXT NOT NULL,
                                PropertyName TEXT NOT NULL,
                                TenantName TEXT,
                                Description TEXT NOT NULL,
                                Priority TEXT NOT NULL,
                                Status TEXT NOT NULL,
                                RequestDate DATETIME NOT NULL,
                                CompletedDate DATETIME,
                                Cost DECIMAL,
                                AssignedTo TEXT
                            )", connection);

                        createTableCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        // Check if all columns exist, if not, you might need to alter the table
                        // This is a simplified check - in production you'd want more robust migration handling
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}");
            }
        }
    }
}