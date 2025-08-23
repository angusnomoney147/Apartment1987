using System;
using Microsoft.Data.Sqlite;

namespace ApartmentManagementSystem
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Data Source=apartment.db";

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string createTenants = @"
                CREATE TABLE IF NOT EXISTS Tenants (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT,
                    Phone TEXT,
                    EmergencyContact TEXT,
                    EmergencyPhone TEXT,
                    NationalId TEXT,
                    DateOfBirth TEXT,
                    Address TEXT,
                    CreatedDate TEXT,
                    IsActive INTEGER DEFAULT 1
                )";

            string createProperties = @"
                CREATE TABLE IF NOT EXISTS Properties (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Address TEXT,
                    City TEXT,
                    Country TEXT,
                    TotalUnits INTEGER DEFAULT 0,
                    ManagerName TEXT,
                    ContactInfo TEXT,
                    CreatedDate TEXT
                )";

            string createUnits = @"
                CREATE TABLE IF NOT EXISTS Units (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PropertyId INTEGER,
                    UnitNumber TEXT NOT NULL,
                    UnitType TEXT,
                    Size REAL,
                    RentAmount REAL,
                    Description TEXT,
                    Status INTEGER DEFAULT 0,
                    Bedrooms INTEGER,
                    Bathrooms INTEGER,
                    FOREIGN KEY (PropertyId) REFERENCES Properties(Id)
                )";

            string createLeases = @"
                CREATE TABLE IF NOT EXISTS Leases (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UnitId INTEGER,
                    TenantId INTEGER,
                    StartDate TEXT,
                    EndDate TEXT,
                    MonthlyRent REAL,
                    SecurityDeposit REAL,
                    Terms TEXT,
                    Status INTEGER DEFAULT 0,
                    CreatedDate TEXT,
                    FOREIGN KEY (UnitId) REFERENCES Units(Id),
                    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
                )";

            string createPayments = @"
                CREATE TABLE IF NOT EXISTS Payments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LeaseId INTEGER,
                    Amount REAL,
                    PaymentDate TEXT,
                    Method INTEGER DEFAULT 0,
                    ReferenceNumber TEXT,
                    Status INTEGER DEFAULT 0,
                    Notes TEXT,
                    FOREIGN KEY (LeaseId) REFERENCES Leases(Id)
                )";

            string createMaintenance = @"
                CREATE TABLE IF NOT EXISTS MaintenanceRequests (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UnitId INTEGER,
                    TenantId INTEGER,
                    Description TEXT,
                    Priority INTEGER DEFAULT 1,
                    Status INTEGER DEFAULT 0,
                    RequestDate TEXT,
                    CompletedDate TEXT,
                    Cost REAL,
                    AssignedTo TEXT,
                    FOREIGN KEY (UnitId) REFERENCES Units(Id),
                    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
                )";

            string createLeaseDocuments = @"
                CREATE TABLE IF NOT EXISTS LeaseDocuments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LeaseId INTEGER,
                    DocumentName TEXT NOT NULL,
                    FilePath TEXT NOT NULL,
                    Type INTEGER DEFAULT 0,
                    UploadDate TEXT,
                    Notes TEXT,
                    FOREIGN KEY (LeaseId) REFERENCES Leases(Id)
                )";

            using var command1 = new SqliteCommand(createTenants, connection);
            command1.ExecuteNonQuery();

            using var command2 = new SqliteCommand(createProperties, connection);
            command2.ExecuteNonQuery();

            using var command3 = new SqliteCommand(createUnits, connection);
            command3.ExecuteNonQuery();

            using var command4 = new SqliteCommand(createLeases, connection);
            command4.ExecuteNonQuery();

            using var command5 = new SqliteCommand(createPayments, connection);
            command5.ExecuteNonQuery();

            using var command6 = new SqliteCommand(createMaintenance, connection);
            command6.ExecuteNonQuery();

            using var command7 = new SqliteCommand(createLeaseDocuments, connection);
            command7.ExecuteNonQuery();
        }

        public static string GetConnectionString() => ConnectionString;
    }
}