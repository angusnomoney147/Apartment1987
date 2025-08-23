using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ApartmentManagementSystem
{
    public class LeaseDocumentRepository
    {
        private readonly string _connectionString;

        public LeaseDocumentRepository()
        {
            _connectionString = DatabaseHelper.GetConnectionString();
            EnsureDocumentsDirectory();
        }

        private void EnsureDocumentsDirectory()
        {
            var documentsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documents");
            if (!Directory.Exists(documentsDir))
            {
                Directory.CreateDirectory(documentsDir);
            }
        }

        public List<LeaseDocument> GetDocumentsByLease(int leaseId)
        {
            var documents = new List<LeaseDocument>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using var command = new SQLiteCommand("SELECT * FROM LeaseDocuments WHERE LeaseId = @LeaseId ORDER BY UploadDate DESC", connection);
            command.Parameters.AddWithValue("@LeaseId", leaseId);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                documents.Add(new LeaseDocument
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    LeaseId = Convert.ToInt32(reader["LeaseId"]),
                    DocumentName = reader["DocumentName"].ToString() ?? string.Empty,
                    FilePath = reader["FilePath"].ToString() ?? string.Empty,
                    Type = (DocumentType)Convert.ToInt32(reader["Type"] ?? 0),
                    UploadDate = DateTime.Parse(reader["UploadDate"]?.ToString() ?? DateTime.Now.ToString()),
                    Notes = reader["Notes"]?.ToString() ?? string.Empty
                });
            }
            return documents;
        }

        public void AddDocument(LeaseDocument document)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            const string query = @"INSERT INTO LeaseDocuments (LeaseId, DocumentName, FilePath, 
                                 Type, UploadDate, Notes) 
                                 VALUES (@LeaseId, @DocumentName, @FilePath, 
                                 @Type, @UploadDate, @Notes)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@LeaseId", document.LeaseId);
            command.Parameters.AddWithValue("@DocumentName", document.DocumentName);
            command.Parameters.AddWithValue("@FilePath", document.FilePath);
            command.Parameters.AddWithValue("@Type", (int)document.Type);
            command.Parameters.AddWithValue("@UploadDate", document.UploadDate.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Notes", document.Notes ?? string.Empty);
            command.ExecuteNonQuery();
        }

        public void DeleteDocument(int id)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // First get the file path to delete the file
            using var selectCommand = new SQLiteCommand("SELECT FilePath FROM LeaseDocuments WHERE Id = @Id", connection);
            selectCommand.Parameters.AddWithValue("@Id", id);
            var filePath = selectCommand.ExecuteScalar()?.ToString();

            // Delete the database record
            using var deleteCommand = new SQLiteCommand("DELETE FROM LeaseDocuments WHERE Id = @Id", connection);
            deleteCommand.Parameters.AddWithValue("@Id", id);
            deleteCommand.ExecuteNonQuery();

            // Delete the actual file if it exists
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignore file deletion errors
                }
            }
        }
    }
}