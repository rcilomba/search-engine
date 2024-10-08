using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace WordService
{
    public class Database
    {
        private static Database _instance;
        private static readonly object _lock = new object();
        private Coordinator coordinator = new Coordinator();

        public static Database GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Database();
                    }
                }
            }
            return _instance;
        }

        private async Task ExecuteAsync(IDbConnection connection, string sql)
        {
            using var trans = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
            trans.Commit();
        }

        public async Task DeleteDatabaseAsync()
        {
            foreach (var connection in coordinator.GetAllConnections())
            {
                await ExecuteAsync(connection, "DROP TABLE IF EXISTS Occurrences");
                await ExecuteAsync(connection, "DROP TABLE IF EXISTS Words");
                await ExecuteAsync(connection, "DROP TABLE IF EXISTS Documents");
            }
        }

        public async Task RecreateDatabaseAsync()
        {
            await ExecuteAsync(coordinator.GetDocumentConnection(), "CREATE TABLE Documents(id INTEGER PRIMARY KEY, url VARCHAR(500))");
            await ExecuteAsync(coordinator.GetOccurrenceConnection(), "CREATE TABLE Occurrences(wordId INTEGER, docId INTEGER)");

            foreach (var connection in coordinator.GetAllWordConnections())
            {
                await ExecuteAsync(connection, "CREATE TABLE Words(id INTEGER PRIMARY KEY, name VARCHAR(500))");
            }
        }

        public async Task<Dictionary<int, int>> GetDocumentsAsync(List<int> wordIds)
        {
            var res = new Dictionary<int, int>();
            var sql = @"SELECT docId, COUNT(wordId) AS count FROM Occurrences WHERE wordId IN " + AsString(wordIds) + " GROUP BY docId ORDER BY count DESC;";

            var connection = coordinator.GetOccurrenceConnection();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = sql;

            using (var reader = await selectCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var docId = reader.GetInt32(0);
                    var count = reader.GetInt32(1);

                    res.Add(docId, count);
                }
            }

            return res;
        }

        private string AsString(List<int> x)
        {
            return string.Concat("(", string.Join(',', x.Select(i => i.ToString())), ")");
        }

        public async Task<Dictionary<string, int>> GetAllWordsAsync()
        {
            var res = new Dictionary<string, int>();

            foreach (var connection in coordinator.GetAllWordConnections())
            {
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT * FROM Words";

                using (var reader = await selectCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = reader.GetInt32(0);
                        var w = reader.GetString(1);

                        res.Add(w, id);
                    }
                }
            }
            return res;
        }

        public async Task<List<string>> GetDocDetailsAsync(List<int> docIds)
        {
            var res = new List<string>();
            var connection = coordinator.GetDocumentConnection();
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Documents WHERE id IN " + AsString(docIds);

            using (var reader = await selectCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var url = reader.GetString(1);
                    res.Add(url);
                }
            }
            return res;
        }

        public async Task InsertAllWordsAsync(Dictionary<string, int> res)
        {
            foreach (var p in res)
            {
                using (var connection = coordinator.GetWordConnection(p.Key))
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = @"INSERT INTO Words(id, name) VALUES(@id,@name)";

                    var paramName = new SqlParameter("@name", p.Key);
                    command.Parameters.Add(paramName);

                    var paramId = new SqlParameter("@id", p.Value);
                    command.Parameters.Add(paramId);

                    await command.ExecuteNonQueryAsync();
                    transaction.Commit();
                }
            }
        }

        public async Task InsertAllOccAsync(int docId, ISet<int> wordIds)
        {
            var connection = coordinator.GetOccurrenceConnection();
            using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"INSERT INTO Occurrences(wordId, docId) VALUES(@wordId,@docId)";

                var paramwordId = command.CreateParameter();
                paramwordId.ParameterName = "wordId";
                command.Parameters.Add(paramwordId);

                var paramDocId = command.CreateParameter();
                paramDocId.ParameterName = "docId";
                paramDocId.Value = docId;
                command.Parameters.Add(paramDocId);

                foreach (var p in wordIds)
                {
                    paramwordId.Value = p;
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
        }

        public async Task InsertDocumentAsync(int id, string url)
        {
            var connection = coordinator.GetDocumentConnection();
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Documents(id, url) VALUES(@id,@url)";

            var pName = new SqlParameter("url", url);
            insertCmd.Parameters.Add(pName);

            var pCount = new SqlParameter("id", id);
            insertCmd.Parameters.Add(pCount);

            await insertCmd.ExecuteNonQueryAsync();
        }
    }
}
