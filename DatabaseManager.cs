using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace ApartmentManagementSystem
{
    public static class DatabaseManager
    {
        private const string DatabaseName = "apartment.db";
        private static string ConnectionString => $"Data Source={DatabaseName};Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                // Only create database if it doesn't exist - THIS FIXES THE ISSUE!
                if (!File.Exists(DatabaseName))
                {
                    SQLiteConnection.CreateFile(DatabaseName);

                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Open();
                        CreateTables(connection);
                    }
                }
                // If database exists, do nothing - keep the data!
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}");
            }
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    // Create Properties table with all columns
                    using (var command = new SQLiteCommand(@"
                        CREATE TABLE Properties (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Address TEXT,
                            City TEXT,
                            State TEXT,
                            ZipCode TEXT,
                            Country TEXT,
                            ManagerName TEXT,
                            ContactInfo TEXT,
                            TotalUnits INTEGER DEFAULT 0,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create Units table
                    using (var command = new SQLiteCommand(@"
                        CREATE TABLE Units (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            PropertyId INTEGER NOT NULL,
                            UnitNumber TEXT NOT NULL,
                            UnitType TEXT,
                            Size DECIMAL,
                            RentAmount DECIMAL,
                            Bedrooms INTEGER,
                            Bathrooms INTEGER,
                            Description TEXT,
                            Status INTEGER DEFAULT 0,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (PropertyId) REFERENCES Properties(Id)
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create Tenants table
                    using (var command = new SQLiteCommand(@"
                        CREATE TABLE Tenants (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            FirstName TEXT NOT NULL,
                            LastName TEXT NOT NULL,
                            Email TEXT,
                            Phone TEXT,
                            EmergencyContact TEXT,
                            EmergencyPhone TEXT,
                            NationalId TEXT,
                            DateOfBirth DATETIME,
                            Address TEXT,
                            CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            IsActive INTEGER NOT NULL DEFAULT 1
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create MaintenanceRequests table
                    using (var command = new SQLiteCommand(@"
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
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create Leases table
                    using (var command = new SQLiteCommand(@"
                        CREATE TABLE Leases (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            TenantId INTEGER NOT NULL,
                            UnitId INTEGER NOT NULL,
                            StartDate DATETIME NOT NULL,
                            EndDate DATETIME NOT NULL,
                            MonthlyRent DECIMAL NOT NULL,
                            SecurityDeposit DECIMAL,
                            Terms TEXT,
                            Status TEXT NOT NULL,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
                            FOREIGN KEY (UnitId) REFERENCES Units(Id)
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create Payments table
                    using (var command = new SQLiteCommand(@"
                        CREATE TABLE Payments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            LeaseId INTEGER NOT NULL,
                            Amount DECIMAL NOT NULL,
                            PaymentDate DATETIME NOT NULL,
                            DueDate DATETIME,
                            Status TEXT NOT NULL,
                            PaymentMethod TEXT,
                            Notes TEXT,
                            CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (LeaseId) REFERENCES Leases(Id)
                        )", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating tables: {ex.Message}");
            }
        }
    }
}