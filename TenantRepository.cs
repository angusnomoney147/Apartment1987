using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System;

namespace ApartmentManagementSystem
{
    public class TenantRepository
    {
        private readonly string _connectionString;

        public TenantRepository()
        {
            _connectionString = DatabaseHelper.GetConnectionString();
        }

        public List<Tenant> GetAll()
        {
            var tenants = new List<Tenant>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM Tenants WHERE IsActive = 1 ORDER BY FirstName, LastName", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tenants.Add(new Tenant
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    FirstName = reader["FirstName"].ToString() ?? string.Empty,
                    LastName = reader["LastName"].ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? string.Empty,
                    Phone = reader["Phone"]?.ToString() ?? string.Empty,
                    EmergencyContact = reader["EmergencyContact"]?.ToString() ?? string.Empty,
                    EmergencyPhone = reader["EmergencyPhone"]?.ToString() ?? string.Empty,
                    NationalId = reader["NationalId"]?.ToString() ?? string.Empty,
                    DateOfBirth = DateTime.Parse(reader["DateOfBirth"]?.ToString() ?? DateTime.Now.ToString()),
                    Address = reader["Address"]?.ToString() ?? string.Empty,
                    CreatedDate = DateTime.Parse(reader["CreatedDate"]?.ToString() ?? DateTime.Now.ToString()),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }
            return tenants;
        }

        public void Add(Tenant tenant)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO Tenants (FirstName, LastName, Email, Phone, 
                                 EmergencyContact, EmergencyPhone, NationalId, DateOfBirth, 
                                 Address, CreatedDate, IsActive) 
                                 VALUES (@FirstName, @LastName, @Email, @Phone, 
                                 @EmergencyContact, @EmergencyPhone, @NationalId, @DateOfBirth, 
                                 @Address, @CreatedDate, @IsActive)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", tenant.FirstName);
            command.Parameters.AddWithValue("@LastName", tenant.LastName);
            command.Parameters.AddWithValue("@Email", tenant.Email ?? string.Empty);
            command.Parameters.AddWithValue("@Phone", tenant.Phone ?? string.Empty);
            command.Parameters.AddWithValue("@EmergencyContact", tenant.EmergencyContact ?? string.Empty);
            command.Parameters.AddWithValue("@EmergencyPhone", tenant.EmergencyPhone ?? string.Empty);
            command.Parameters.AddWithValue("@NationalId", tenant.NationalId ?? string.Empty);
            command.Parameters.AddWithValue("@DateOfBirth", tenant.DateOfBirth.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Address", tenant.Address ?? string.Empty);
            command.Parameters.AddWithValue("@CreatedDate", DateTime.Now.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@IsActive", tenant.IsActive ? 1 : 0);
            command.ExecuteNonQuery();
        }

        public void Update(Tenant tenant)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"UPDATE Tenants SET FirstName = @FirstName, LastName = @LastName, 
                                 Email = @Email, Phone = @Phone, 
                                 EmergencyContact = @EmergencyContact, EmergencyPhone = @EmergencyPhone,
                                 NationalId = @NationalId, DateOfBirth = @DateOfBirth, 
                                 Address = @Address, IsActive = @IsActive
                                 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", tenant.Id);
            command.Parameters.AddWithValue("@FirstName", tenant.FirstName);
            command.Parameters.AddWithValue("@LastName", tenant.LastName);
            command.Parameters.AddWithValue("@Email", tenant.Email ?? string.Empty);
            command.Parameters.AddWithValue("@Phone", tenant.Phone ?? string.Empty);
            command.Parameters.AddWithValue("@EmergencyContact", tenant.EmergencyContact ?? string.Empty);
            command.Parameters.AddWithValue("@EmergencyPhone", tenant.EmergencyPhone ?? string.Empty);
            command.Parameters.AddWithValue("@NationalId", tenant.NationalId ?? string.Empty);
            command.Parameters.AddWithValue("@DateOfBirth", tenant.DateOfBirth.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Address", tenant.Address ?? string.Empty);
            command.Parameters.AddWithValue("@IsActive", tenant.IsActive ? 1 : 0);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = "UPDATE Tenants SET IsActive = 0 WHERE Id = @Id";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }
    }
}